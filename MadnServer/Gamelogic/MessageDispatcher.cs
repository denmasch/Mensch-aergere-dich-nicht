using System;
using System.Threading.Tasks;
using MadnServer.Player;
using MadnShared.Enums;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;
using MadnShared.Messages.Errors;
using MadnServer.Services;

namespace MadnServer.Gamelogic;

public class MessageDispatcher : IMessageDispatcher
{
    private readonly ILogger _logger;
    private readonly IGameManager _gameManager;
    private readonly IStatsService _statsService;

    public MessageDispatcher(ILogger logger, IGameManager  gameManager, IStatsService statsService)
    {
        _logger = logger;
        _gameManager = gameManager;
        _statsService = statsService;
    }
    
    public async Task DispatchAsync(IPlayer fromPlayer, IMessage message)
    {
        if (message == null)
            return;

        _logger.LogInfo($"Dispatching message of type {message.GetType().Name}");

        if (message is IGameMessage gameMessage)
        {
            var game = _gameManager.GetGame(gameMessage.GameId);
            if (game != null)
            {
                game.HandleMessage(fromPlayer, gameMessage);
            }
            else
            {
                _logger.LogInfo($"No game found with id {gameMessage.GameId}. Sending error to sender.");
                await fromPlayer.SendAsync(new UnknownMessageTypeMessage());
            }
            return;
        }

        if (message is ILobbyMessage lobbyMessage)
        {
            await HandleLobbyMessageAsync(fromPlayer, lobbyMessage);
            return;
        }

        _logger.LogError($"Received unknown message type: {message.GetType().Name}");
        await fromPlayer.SendAsync(new UnknownMessageTypeMessage());
    }

    private async Task HandleLobbyMessageAsync(IPlayer fromPlayer, ILobbyMessage lobbyMessage)
    {
        var typeName = lobbyMessage.GetType().Name;

        _logger.LogInfo($"Handling lobby message of type {typeName} from player {fromPlayer.Id}");
        
        Game game;
        Color color;
        switch (lobbyMessage)
        {
            case CreateGameMessage createGameMessage:
                game = _gameManager.CreateGame(fromPlayer);
                color = game.Players.Find(p => p.Id == fromPlayer.Id).Color;
                await fromPlayer.SendAsync(new GameCreatedMessage { GameId = game.Id , Gameboard = game.Gameboard.ToDto(), Color = color });
                await fromPlayer.SendAsync(new GameInfoMessage() {GameId = game.Id, AdminColor = game.Players[0].Color, PlayerCount = game.Players.Count});
                break;
            case JoinGameMessage joinGameMessage:
                game = _gameManager.TryJoinGame(joinGameMessage.GameId, fromPlayer);
                color = game.Players.Find(p => p.Id == fromPlayer.Id).Color;
                await fromPlayer.SendAsync(new GameJoinedMessage() { GameId = joinGameMessage.GameId , Gameboard = game.Gameboard.ToDto(), Color = color });
                await fromPlayer.SendAsync(new GameInfoMessage() {GameId = game.Id, AdminColor = game.Players[0].Color, PlayerCount = game.Players.Count});
                break;
            case ListGamesMessage listGamesMessage:
                var games = _gameManager.GetAllJoinableGames();
                await fromPlayer.SendAsync(new ListGamesResponseMessage { Games = games });
                break;
            case ListMatchHistoryMessage listMatchHistoryMessage:
                // Fetch stored matches from StatsService and send back to client
                var matches = _statsService.GetStoredMatches();
                await fromPlayer.SendAsync(new MatchHistoryResponseMessage { Matches = matches });
                break;
            default:
                _logger.LogError($"Unhandled lobby message type: {typeName}");
                break;
                
        }
    }
}
