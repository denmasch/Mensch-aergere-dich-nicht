using System;
using System.Threading.Tasks;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;
using MadnShared.Logger;
using MadnShared.Enums;

namespace MadnClient
{
    public class GameManager
    {
        private readonly IWebSocketClient _wsClient;
        private readonly MessageDispatcher _dispatcher;
        private readonly ILogger _logger;

        private TaskCompletionSource<DiceResultMessage> _diceTcs;
        private TaskCompletionSource<GameLeftMessage> _leaveTcs;

        public GameManager(IWebSocketClient wsClient, MessageDispatcher dispatcher, ILogger logger)
        {
            _wsClient = wsClient;
            _dispatcher = dispatcher;
            _logger = logger;

            _dispatcher.DiceResultReceived += msg => _diceTcs?.TrySetResult(msg);
            _dispatcher.GameLeftReceived += msg => _leaveTcs?.TrySetResult(msg);
        }

        public async Task SendAsync(IMessage message)
        {
            try
            {
                await _wsClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Cannot send message: " + ex.Message);
            }
        }

        public async Task<DiceResultMessage> SendRollDiceAsync(Guid gameId, Guid playerId)
        {
            try
            {
                _diceTcs = new TaskCompletionSource<DiceResultMessage>();
                var msg = new RollDiceMessage() { GameId = gameId, PlayerId = playerId };
                await SendAsync(msg);
                _logger.LogInfo("Sent RollDice message");
                return await _diceTcs.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception when sending RollDice message: " + ex.Message);
                return null;
            }
        }

        public async Task<GameLeftMessage> SendLeaveAsync(Guid gameId, Guid playerId)
        {
            try
            {
                _leaveTcs = new TaskCompletionSource<GameLeftMessage>();
                var msg = new LeaveGameMessage() { GameId = gameId, PlayerId = playerId };
                await SendAsync(msg);
                _logger.LogInfo("Sent Leave message");
                return await _leaveTcs.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception when sending LeaveGame message: " + ex.Message);
                return null;
            }
        }

        public async Task SendAddCpuAsync(Guid gameId, Difficulty difficulty)
        {
            try
            {
                var msg = new AddCpuPlayerMessage() { GameId = gameId, Difficulty = difficulty };
                await SendAsync(msg);
                _logger.LogInfo("Sent AddCpuPlayer message");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception when sending AddCpuPlayer message: " + ex.Message);
            }
        }

        public async Task SendStartGameAsync(Guid gameId, Guid playerId)
        {
            try
            {
                var msg = new StartGameMessage() { GameId = gameId, PlayerId = playerId };
                await SendAsync(msg);
                _logger.LogInfo("Sent StartGame message");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception when sending StartGame message: " + ex.Message);
            }
        }

        public async Task SendMoveFigureAsync(Guid gameId, Guid playerId, int figureId, int diceRoll)
        {
            try
            {
                var msg = new MoveFigureMessage() { GameId = gameId, PlayerId = playerId, FigureId = figureId, DiceRoll = diceRoll };
                await SendAsync(msg);
                _logger.LogInfo("Sent MoveFigure message");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception when sending MoveFigure message: " + ex.Message);
            }
        }
    }
}

