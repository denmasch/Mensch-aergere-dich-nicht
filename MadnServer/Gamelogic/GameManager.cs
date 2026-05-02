using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MadnServer.Player;
using MadnServer.Services;
using MadnShared.Logger;

namespace MadnServer.Gamelogic;

/// <summary>
/// Manages all active games in the server.
/// </summary>
public class GameManager : IGameManager
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();
    private readonly ILogger _logger;
    private readonly IStatsService _statsService;

    public GameManager(ILogger logger, IStatsService statsService)
    {
        _logger = logger;
        _statsService = statsService;
    }

    public Game CreateGame(IPlayer player)
    {
        var game = new Game(new List<IPlayer>(){player}, _logger,  _statsService);
        game.GameFinished += RemoveGame;
        _games[game.Id] = game;
        _logger.LogInfo($"Created new game {game.Id}.");
        return game;
    }

    public Game? GetGame(Guid gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
    }

    public Dictionary<Guid, int> GetAllJoinableGames()
    {
        return _games
            .Where(kvp => kvp.Value != null && !kvp.Value.IsStarted && kvp.Value.Players.Count < 4)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Players.Count);
    }

    public void RemoveGame(Guid gameId)
    {
        _logger.LogInfo($"Removing game {gameId} from GameManager.");
        _games.TryRemove(gameId, out _);
    }

    public Game TryJoinGame(Guid gameId, IPlayer player)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return null;
        
        game.AddPlayer(player);
        return game;
    }
    public void RemovePlayerFromGames(IPlayer player)
    {
        foreach (var game in _games.Values.ToList())
        {
            if (game.Players.Any(p => p.Id == player.Id))
            {
                _logger.LogInfo($"Removing player {player.Id} from game {game.Id} due to disconnect.");
                game.RemovePlayer(player);
                // Remove the game if all remaining players are CPU players
                if (game.Players.All(p => p is ICpuPlayer))
                {
                   RemoveGame(game.Id);
                }
                break;
            }
        }
    }
}