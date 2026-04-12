using System;
using MadnServer.Player;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;
using MadnShared.Messages.Errors;

namespace MadnServer.Gamelogic;

public static class MessageDispatcher
{
    public static void Dispatch(IPlayer fromPlayer, IMessage message)
    {
        if (message == null)
            return;

        Logger.LogInfo($"Dispatching message of type {message.GetType().Name}");

        if (message is IGameMessage gameMessage)
        {
            var game = GameManager.GetGame(gameMessage.GameId);
            if (game != null)
            {
                game.HandleMessage(fromPlayer, gameMessage);
            }
            else
            {
                Logger.LogInfo($"No game found with id {gameMessage.GameId}. Sending error to sender.");
                fromPlayer.SendAsync(new UnknownMessageTypeMessage());
            }
            return;
        }

        if (message is ILobbyMessage lobbyMessage)
        {
            HandleLobbyMessage(fromPlayer, lobbyMessage);
            return;
        }

        Logger.LogError($"Received unknown message type: {message.GetType().Name}");
        fromPlayer.SendAsync(new UnknownMessageTypeMessage());
    }

    private static void HandleLobbyMessage(IPlayer fromPlayer, ILobbyMessage lobbyMessage)
    {
        var typeName = lobbyMessage.GetType().Name;

        Logger.LogInfo($"Handling lobby message of type {typeName} from player {fromPlayer.Id}");
        
        Game game;
        switch (lobbyMessage)
        {
            case CreateGameMessage createGameMessage:
                game = GameManager.CreateGame(fromPlayer);
                fromPlayer.SendAsync(new GameCreatedMessage { GameId = game.Id , Gameboard = game.Gameboard.ToDto(), Color = fromPlayer.Color });
                break;
            case JoinGameMessage joinGameMessage:
                game = GameManager.TryJoinGame(joinGameMessage.GameId, fromPlayer);
                fromPlayer.SendAsync(new GameJoinedMessage() { GameId = joinGameMessage.GameId , Gameboard = game.Gameboard.ToDto(), Color = fromPlayer.Color });
                break;
            case ListGamesMessage listGamesMessage:
                var games = GameManager.GetAllGames();
                fromPlayer.SendAsync(new ListGamesResponseMessage { Games = games });
                break;
            default:
                Logger.LogError($"Unhandled lobby message type: {typeName}");
                break;
                
        }
    }
}
