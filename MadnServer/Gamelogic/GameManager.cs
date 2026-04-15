using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MadnServer.Player;
using MadnShared.Logger;

namespace MadnServer.Gamelogic;

/// <summary>
/// Manages all active games in the server.
/// </summary>
public static class GameManager
{
    private static readonly ConcurrentDictionary<Guid, Game> _games = new();

    public static Game CreateGame(IPlayer player)
    {
        var game = new Game(new List<IPlayer>(){player});
        _games[game.Id] = game;
        Logger.LogInfo($"Created new game {game.Id}.");
        return game;
    }

    public static Game? GetGame(Guid gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
    }

    public static Dictionary<Guid, int> GetAllJoinableGames()
    {
        return _games
            .Where(kvp => kvp.Value != null && !kvp.Value.IsStarted && kvp.Value.Players.Count < 4)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Players.Count);
    }

    public static bool RemoveGame(Guid gameId)
    {
        Logger.LogInfo($"Removing game {gameId} from GameManager.");
        return _games.TryRemove(gameId, out _);
    }

    public static Game TryJoinGame(Guid gameId, IPlayer player)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return null;
        
        game.AddPlayer(player);
        return game;
    }
    public static void RemovePlayerFromGames(IPlayer player)
    {
        foreach (var game in _games.Values.ToList())
        {
            if (game.Players.Any(p => p.Id == player.Id))
            {
                Logger.LogInfo($"Removing player {player.Id} from game {game.Id} due to disconnect.");
                game.RemovePlayer(player);
                break;
            }
        }
    }
}