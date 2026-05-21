using System;
using MadnShared.Messages.Base;
using MadnShared.Messages.ServerToClient;
using MadnShared.Logger;

namespace MadnClient
{
    public class MessageDispatcher
    {
        private readonly IWebSocketClient _wsClient;
        private readonly ILogger _logger;

        public event Action<WelcomeMessage> WelcomeReceived;
        public event Action<GameCreatedMessage> GameCreatedReceived;
        public event Action<ListGamesResponseMessage> ListGamesReceived;
        public event Action<GameJoinedMessage> GameJoinedReceived;
        public event Action<MatchHistoryResponseMessage> MatchHistoryReceived;

        // game events
        public event Action<DiceResultMessage> DiceResultReceived;
        public event Action<GameboardUpdatedMessage> GameboardUpdatedReceived;
        public event Action<GameLeftMessage> GameLeftReceived;
        public event Action<NextPlayerMessage> NextPlayerReceived;
        public event Action<GameOverMessage> GameOverReceived;
        public event Action<GameInfoMessage> GameInfoReceived;

        public MessageDispatcher(IWebSocketClient wsClient, ILogger logger)
        {
            _wsClient = wsClient;
            _logger = logger;
            _wsClient.MessageReceived += OnWsMessageReceived;
        }

        private void OnWsMessageReceived(IMessage message)
        {
            _logger.LogInfo($"Dispatcher received: {message?.GetType().Name}");
            switch (message)
            {
                case WelcomeMessage w: WelcomeReceived?.Invoke(w); break;
                case GameCreatedMessage gc: GameCreatedReceived?.Invoke(gc); break;
                case ListGamesResponseMessage lg: ListGamesReceived?.Invoke(lg); break;
                case GameJoinedMessage gj: GameJoinedReceived?.Invoke(gj); break;
                case MatchHistoryResponseMessage mh: MatchHistoryReceived?.Invoke(mh); break;

                case DiceResultMessage dr: DiceResultReceived?.Invoke(dr); break;
                case GameboardUpdatedMessage gb: GameboardUpdatedReceived?.Invoke(gb); break;
                case GameLeftMessage gl: GameLeftReceived?.Invoke(gl); break;
                case NextPlayerMessage np: NextPlayerReceived?.Invoke(np); break;
                case GameOverMessage go: GameOverReceived?.Invoke(go); break;
                case GameInfoMessage gi: GameInfoReceived?.Invoke(gi); break;
                default:
                    _logger.LogInfo($"Unhandled message type in dispatcher: {message?.GetType().Name}");
                    break;
            }
        }
    }
}

