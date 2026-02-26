using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MadnServer.Player;

namespace MadnServer.Gamelogic;

/// <summary>
/// Manages all active games in the server.
/// </summary>
public static class GameManager
{
    private static readonly ConcurrentDictionary<Guid, Game> _games = new();

    public static Game CreateGame()
    {
        var gameId = Guid.NewGuid();
        var game = new Game(gameId, new List<IPlayer>());
        _games[gameId] = game;
        return game;
    }

    public static Game? GetGame(Guid gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
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