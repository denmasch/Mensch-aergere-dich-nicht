using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MadnServer.Player;

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
        return game;
    }

    public static Game? GetGame(Guid gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
    }

    public static Dictionary<Guid, int> GetAllGames()
    {
        return _games.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Players.Count);
    }

    public static bool RemoveGame(Guid gameId)
    {
        return _games.TryRemove(gameId, out _);
    }

    public static bool TryJoinGame(Guid gameId, IPlayer player)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return false;

        game.AddPlayer(player);
        return true;
    }
}