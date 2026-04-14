using System;
using System.Threading.Tasks;
using MadnShared.Messages.ServerToClient;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Enums;
using MadnShared.GameAssets;

namespace MadnClient
{
    public class GameFrontend
    {
        private readonly IWebSocketClient _wsClient;
        private readonly Guid _playerId;
        private Guid _gameId;
        private Color _currentTurnColor;
        private Color _yourColor;
        private Color? _winnerColor;
        private int _currentTurnDice;
        private bool _isGameStarted = false;
        private GameboardDTO _currentGameboard;
        private TaskCompletionSource<DiceResultMessage> _diceTcs;
        private TaskCompletionSource<GameLeftMessage> _leaveTcs;
        private GameState _gameState;
        private bool _needsRedraw = true;

        /// <summary>
        /// Predefined mapping of (x,y) coordinates to path indices for the 11x11 board.
        /// </summary>
        private static readonly Dictionary<(int x, int y), int> _pathMap = new Dictionary<(int, int), int>
        {
            {(0,4), 0}, {(1,4), 1}, {(2,4), 2}, {(3,4), 3}, {(4,4), 4},
            {(4,3), 5}, {(4,2), 6}, {(4,1), 7}, {(4,0), 8}, {(5,0), 9},
            {(6,0), 10}, {(6,1), 11}, {(6,2), 12}, {(6,3), 13}, {(6,4), 14},
            {(7,4), 15}, {(8,4), 16}, {(9,4), 17}, {(10,4), 18}, {(10,5), 19},
            {(10,6), 20}, {(9,6), 21}, {(8,6), 22}, {(7,6), 23}, {(6,6), 24},
            {(6,7), 25}, {(6,8), 26}, {(6,9), 27}, {(6,10), 28}, {(5,10), 29},
            {(4,10), 30}, {(4,9), 31}, {(4,8), 32}, {(4,7), 33}, {(4,6), 34},
            {(3,6), 35}, {(2,6), 36}, {(1,6), 37}, {(0,6), 38}, {(0,5), 39}
        };
        /// <summary>
        /// reverse map pathIndex -> (x,y)
        /// </summary>
        private static readonly Dictionary<int, (int x, int y)> _indexToCoord;

        private List<Move> _currentValidMoves;
        private int _selectedMoveIndex = 0;
        private (int x, int y)? _highlightStart;
        private (int x, int y)? _highlightTarget;

        static GameFrontend()
        {
            // reverse the map
            _indexToCoord = _pathMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }

        public GameFrontend(IWebSocketClient wsClient, Guid playerId, Color yourColor, GameboardDTO initialBoard)
        {
            _wsClient = wsClient;
            _playerId = playerId;
            _yourColor = yourColor;
            _currentGameboard = initialBoard;
            _wsClient.MessageReceived += OnWsMessageReceived;
        }

        public async Task EnterGameAsync(Guid gameId)
        {
            _gameId = gameId;
            
            bool stay = true;
            while (stay)
            {
                if (_needsRedraw)
                {
                    ShowMenu();
                    _needsRedraw = false;
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.B:
                            if (_gameState == GameState.GameOver)
                            {
                                stay = false;
                                break;
                            }
                            var res = await SendLeaveAsync();
                            if (res != null && res.PlayerId == _playerId)
                            {
                                stay = false;
                            }

                            break;
                        case ConsoleKey.S:
                            if (_gameState != GameState.WaitingForStart)
                            {
                                Console.WriteLine("Spiel wurde bereits gestartet.");
                                break;
                            }
                            await SendStartGameAsync();
                            break;
                        case ConsoleKey.W:
                            if (_gameState != GameState.RollDice)
                            {
                                Console.WriteLine("Du kannst nur würfeln, wenn du am Zug bist und noch nicht gewürfelt hast.");
                                break;
                            }

                            var response = await SendRollDiceAsync();
                            if (response != null && response.ValidMoves != null && response.ValidMoves.Count > 0)
                            {
                                await PromptSelectMoveAsync(response);
                            }
                            else
                            {
                                ShowMenu();
                                Console.WriteLine("Keine gültigen Züge vorhanden.");
                            }

                            break;
                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            Console.WriteLine("Figur kann nur nach dem Würfeln ausgewählt werden.");
                            break;
                        case ConsoleKey.Enter:
                            Console.WriteLine("Figur kann nur nach dem Würfeln bewegt werden.");
                            break;
                        default:
                            Console.WriteLine("Unbekannte Option. 'B' zum Zurückkehren.");
                            break;
                    }
                }

                await Task.Delay(100);
            }

            Console.Clear();
            Console.WriteLine("Zurück zum Menü...");
            await Task.Delay(300);
        }
        
        private async Task SendMessageAsync(IMessage message)
        {
            try
            {
                await _wsClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError("Cannot send message: " + ex.Message);
            }
        }

        private async Task<GameLeftMessage> SendLeaveAsync()
        {
            try
            {
                _leaveTcs = new TaskCompletionSource<GameLeftMessage>();
                var leaveGameMessage = new LeaveGameMessage()
                {
                    GameId = _gameId,
                    PlayerId = _playerId
                };

                await SendMessageAsync(leaveGameMessage);
                Logger.LogInfo($"Sent Leave message");
                return await _leaveTcs.Task;
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception when sending LeaveGame message: " + ex.Message);
                return null;
            }
        }
        
        private async Task<DiceResultMessage> SendRollDiceAsync()
        {
            try
            {
                _diceTcs = new TaskCompletionSource<DiceResultMessage>();
                var rollDiceMessage = new RollDiceMessage()
                {
                    GameId = _gameId,
                    PlayerId = _playerId
                };

                await SendMessageAsync(rollDiceMessage);
                Logger.LogInfo($"Sent RollDice message");
                return await _diceTcs.Task;
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception when sending RollDice message: " + ex.Message);
                return null;
            }
        }
        
        private async Task SendStartGameAsync()
        {
            try
            {
                var startGameMessage = new StartGameMessage()
                {
                    GameId = _gameId,
                    PlayerId = _playerId
                };

                await SendMessageAsync(startGameMessage);
                Logger.LogInfo($"Sent CreateGame message");
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception when sending CreateGame message: " + ex.Message);
            }
        }

        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine($"Spiel: {_gameId}");
            Console.WriteLine();
            Console.WriteLine($"Deine Farbe: {ColorToString(_yourColor)}");
            Console.WriteLine($"Status: {Status()} ");
            Console.WriteLine($"Würfel: {DiceRoll()}");
            DrawGameBoard(_currentGameboard);
            if (_gameState == GameState.GameOver)
            {
                Console.WriteLine();
                Console.WriteLine("Spiel vorbei!");
                if (_winnerColor.HasValue)
                {
                    Console.WriteLine($"Der Gewinner ist {ColorToString(_winnerColor.Value)}.");
                }
                Console.WriteLine("Drück 'b' um zum Menü zurück zu kehren.");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Optionen:");
                Console.WriteLine("B) Spiel verlassen");
                Console.WriteLine("S) Spiel starten");
                Console.WriteLine("W) Würfeln");
                Console.WriteLine("A/D) Figur auswählen");
                Console.WriteLine("Enter) Figur bewegen");
            }
        }

        private string Status()
        {
            switch (_gameState)
            {
                case GameState.WaitingForStart:
                    return "Warte auf Spielstart";
                case GameState.OpponentTurn:
                    return $"Farbe am Zug: {ColorToString(_currentTurnColor)} (Der Gegner ist am Zug)";
                case GameState.RollDice:
                    return $"Farbe am Zug: {ColorToString(_currentTurnColor)} (Du bist am Zug) - Würfeln!";
                case GameState.MoveFigure:
                    return $"Farbe am Zug: {ColorToString(_currentTurnColor)} (Du bist am Zug) - Figur bewegen!";
                case GameState.GameOver:
                    return $"Spiel vorbei!";
                default:
                    return "";
            }
        }
        
        private string ColorToString(Color color)
        {
            switch (color)
            {
                case Color.Yellow: return "Gelb";
                case Color.Green: return "Grün";
                case Color.Blue: return "Blau";
                case Color.Red: return "Rot";
            }
            return "";
        }
        
        private string DiceRoll()
        {
            switch (_currentTurnDice)
             {
                 case 1: return "⚀ (1)";
                 case 2: return "⚁ (2)";
                 case 3: return "⚂ (3)";
                 case 4: return "⚃ (4)";
                 case 5: return "⚄ (5)";
                 case 6: return "⚅ (6)";
                 default: return "-";
             }
        }
        
        private void DrawGameBoard(GameboardDTO board)
        {
            Console.WriteLine();
            
            for (int y = 0; y < 11; y++)
            {
                for (int x = 0; x < 11; x++)
                {
                    DrawTileAt(x, y, board);
                }
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        private void DrawTileAt(int x, int y, GameboardDTO board)
        {
            ConsoleColor bg = ConsoleColor.Black;
            ConsoleColor fg = ConsoleColor.White;
            string symbol = "  ";

            // The Homes 
            if (x <= 1 && y <= 1) { bg = ConsoleColor.DarkYellow; fg = ConsoleColor.Yellow; } // top left
            else if (x >= 9 && y <= 1) { bg = ConsoleColor.DarkGreen; fg = ConsoleColor.Green; } // top right
            else if (x <= 1 && y >= 9) { bg = ConsoleColor.DarkBlue; fg = ConsoleColor.Blue; } // bottom left
            else if (x >= 9 && y >= 9) { bg = ConsoleColor.DarkRed; fg = ConsoleColor.Red; } // bottom right
            
            // The Path
            else if ((x >= 4 && x <= 6) || (y >= 4 && y <= 6)) 
            { 
                bg = ConsoleColor.Gray; 
                fg = ConsoleColor.DarkGray; 
            }
            
            // Get Target Tile
            if (x == 5 && y == 5) { bg = ConsoleColor.Black; fg = ConsoleColor.White; }
            else if (x >= 1 && x <= 5 && y == 5) { bg = ConsoleColor.DarkYellow; fg = ConsoleColor.Yellow; }
            else if (x == 5 && y >= 1 && y <= 5 ) { bg = ConsoleColor.DarkGreen; fg = ConsoleColor.Green; }
            else if (x >= 5 && x <= 9 && y == 5) { bg = ConsoleColor.DarkRed; fg = ConsoleColor.Red; }
            else if (x == 5 && y >= 5 && y <= 9) { bg = ConsoleColor.DarkBlue; fg = ConsoleColor.Blue; }

            TileDTO tile = GetTileFromBoard(x, y, board);
            if (tile != null && tile.Type is TileType.Start)
            {
                bg = GetBackroundColor(tile.Color);
            }
            if (tile != null && tile.IsOccupied) 
            { 
                symbol = "♟ "; 
                fg = GetFigureColor(tile.OccupyingFigure.Color);
            }

            // Highlight start/target of selected move
            if (_highlightStart.HasValue && _highlightStart.Value.x == x && _highlightStart.Value.y == y)
            {
                bg = ConsoleColor.DarkGray;
            }
            else if (_highlightTarget.HasValue && _highlightTarget.Value.x == x && _highlightTarget.Value.y == y)
            {
                bg = ConsoleColor.DarkGray;
            }

            if (bg != ConsoleColor.Black) 
            {
                Console.BackgroundColor = bg;
                Console.ForegroundColor = fg;
                Console.Write(symbol);
            }
            else
            {
                Console.ResetColor();
                Console.Write("  ");
            }
            Console.ResetColor();
            Console.Write(" ");
        }

        private TileDTO GetTileFromBoard(int x, int y, GameboardDTO board)
        {
            if (board == null) return null;

            // 1. Check Homes
            // calculate indices based on the coordinates
            if (x <= 1 && y <= 1) return GetArrayItem(board.Homes, Color.Yellow, x + (y * 2));
            if (x >= 9 && y <= 1) return GetArrayItem(board.Homes, Color.Green, (x - 9) + (y * 2));
            if (x <= 1 && y >= 9) return GetArrayItem(board.Homes, Color.Blue, x + ((y - 9) * 2));
            if (x >= 9 && y >= 9) return GetArrayItem(board.Homes, Color.Red, (x - 9) + ((y - 9) * 2));

            // 2. Check Targets
            // calculate indices based on the coordinates
            if (y == 5 && x >= 1 && x <= 4) return GetArrayItem(board.Targets, Color.Yellow, x - 1);
            if (x == 5 && y >= 1 && y <= 4) return GetArrayItem(board.Targets, Color.Green, y - 1);
            if (y == 5 && x >= 6 && x <= 9) return GetArrayItem(board.Targets, Color.Red, 9 - x);
            if (x == 5 && y >= 6 && y <= 9) return GetArrayItem(board.Targets, Color.Blue, 9 - y);

            // 3. Check Path
            if (_pathMap.TryGetValue((x, y), out int pathIndex) && pathIndex < board.Path.Length)
                return board.Path[pathIndex];

            return null;
        }

        /// <summary>
        /// Gets the tile from the specified dictionary based on color and index
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="c"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private TileDTO GetArrayItem(Dictionary<Color, TileDTO[]> dict, Color c, int index)
        {
            if (dict != null && dict.TryGetValue(c, out var arr) && arr != null && index >= 0 && index < arr.Length)
                return arr[index];
            return null;
        }

        private ConsoleColor GetFigureColor(Color color)
        {
            return color switch
            {
                Color.Yellow => ConsoleColor.Yellow,
                Color.Green => ConsoleColor.Green,
                Color.Blue => ConsoleColor.Blue,
                Color.Red => ConsoleColor.Red,
                _ => ConsoleColor.Black
            };
        }

        private ConsoleColor GetBackroundColor(Color color)
        {
            return color switch
            {
                Color.Yellow => ConsoleColor.DarkYellow,
                Color.Green => ConsoleColor.DarkGreen,
                Color.Blue => ConsoleColor.DarkBlue,
                Color.Red => ConsoleColor.DarkRed,
                _ => ConsoleColor.Gray
            };
        }
        private void OnWsMessageReceived(IMessage message)
        {
            Logger.LogInfo($"Received message: {message}");
            switch (message)
            {
                case DiceResultMessage diceMsg:
                    Logger.LogInfo($"Dice rolled: {diceMsg.Value}");
                    _currentTurnDice = diceMsg.Value;
                    if (diceMsg.ValidMoves != null && diceMsg.ValidMoves.Count > 0)
                        _gameState = GameState.MoveFigure;
                    _diceTcs?.TrySetResult(diceMsg);
                    break;
                case GameboardUpdatedMessage boardMsg:
                    _currentGameboard = boardMsg.Gameboard;
                    break;
                case GameLeftMessage leftMsg:
                    if (leftMsg.PlayerId != _playerId)
                    {
                        Logger.LogInfo($"Player {leftMsg.PlayerId} left the game.");
                        break;
                    }
                    Logger.LogInfo($"Left game with ID: {leftMsg.GameId}");
                    _leaveTcs?.TrySetResult(leftMsg);
                    break;
                case NextPlayerMessage nextMsg:
                    Logger.LogInfo($"Next player: {nextMsg.NextPlayerId}");
                    _isGameStarted = true;
                    _currentTurnColor = nextMsg.NextPlayerColor;
                    if (nextMsg.NextPlayerId != _playerId)
                    {
                        _gameState = GameState.OpponentTurn;
                    }
                    else
                    {
                        _gameState = GameState.RollDice;
                        Logger.LogInfo("Your turn");
                    }
                    break;
                case GameOverMessage gameOverMsg:
                    Logger.LogInfo($"Game over. Winner: {gameOverMsg.WinnerPlayerId}");
                    _isGameStarted = false;
                    _gameState = GameState.GameOver;
                    _winnerColor = gameOverMsg.WinnerColor;
                    break;
            }
            _needsRedraw = true;
        }

        /// <summary>
        /// Prompts the player to select one of the valid moves using A/D or arrow keys.
        /// Enter confirms the currently selected move and sends it to server.
        /// </summary>
        private async Task PromptSelectMoveAsync(DiceResultMessage dice)
        {
            _currentValidMoves = dice.ValidMoves;
            if (_currentValidMoves.Count == 0) 
                return;
            _selectedMoveIndex = 0;
            ComputeHighlights(); 

            bool selecting = true;
            while (selecting)
            {
                Console.Clear();
                ShowMenu();
                
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        _selectedMoveIndex = (_selectedMoveIndex - 1 + _currentValidMoves.Count) % _currentValidMoves.Count;
                        ComputeHighlights();
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        _selectedMoveIndex = (_selectedMoveIndex + 1) % _currentValidMoves.Count;
                        ComputeHighlights();
                        break;
                    case ConsoleKey.Enter:
                        var chosen = _currentValidMoves[_selectedMoveIndex];
                        var moveFigure = new MoveFigureMessage()
                        {
                            GameId = _gameId,
                            PlayerId = _playerId,
                            FigureId = chosen.FigureIndex,
                            DiceRoll = chosen.Steps
                        };
                        await SendMessageAsync(moveFigure);
                        selecting = false;
                        _currentValidMoves = null;
                        _highlightStart = null;
                        _highlightTarget = null;
                        break;
                    case ConsoleKey.Escape:
                    case ConsoleKey.B:
                        selecting = false;
                        _currentValidMoves = null;
                        _highlightStart = null;
                        _highlightTarget = null;
                        break;
                }
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// Compute highlight coordinates for current selected move.
        /// Uses reflection to identify figure indices on tiles and the path reverse-map to compute target.
        /// </summary>
        private void ComputeHighlights()
        {
            _highlightStart = null;
            _highlightTarget = null;
            if (_currentValidMoves == null || _currentValidMoves.Count == 0 || _currentGameboard == null) 
                return;

            var selected = _currentValidMoves[_selectedMoveIndex];

            
            (int x, int y)? start = FindTileCoordinatesForFigureIndex(_currentGameboard, selected.FigureIndex);
            if (start.HasValue)
            {
                _highlightStart = start.Value;


                if (_pathMap.TryGetValue((start.Value.x, start.Value.y), out int startPathIndex))
                {
                    // The Figure is on the Path
                    var path = _currentGameboard.Path;
                    TileDTO startTile = path[startPathIndex];
                    var figColor = startTile.OccupyingFigure.Color;

                    bool enteredTarget = false;
                    int pathIndex = startPathIndex;

                    for (int step = 1; step <= selected.Steps; step++)
                    {
                        pathIndex = (pathIndex + 1) % path.Length;
                        var pTile = path[pathIndex];
                        
                        // if figure enters targets, highlight Targets
                        if (pTile != null && pTile.Type == TileType.Start && pTile.Color == figColor)
                        {
                            int remaining = selected.Steps - step;
                            if (_currentGameboard.Targets != null && _currentGameboard.Targets.TryGetValue(figColor, out var targetArr) && targetArr != null)
                            {
                                if (remaining >= 0 && remaining < targetArr.Length)
                                {
                                    var targetTile = targetArr[remaining];
                                    var coord = FindCoordinatesByTileReference(_currentGameboard, targetTile);
                                    if (coord.HasValue)
                                    {
                                        _highlightTarget = coord.Value;
                                    }
                                }
                            }
                            enteredTarget = true;
                            break;
                        }
                    }

                    if (!enteredTarget)
                    {
                        // no enter into targets -> normal path landing
                        int finalIndex = (startPathIndex + selected.Steps) % path.Length;
                        if (_indexToCoord.TryGetValue(finalIndex, out var coord))
                        {
                            _highlightTarget = coord;
                        }
                    }
                
                }
                else
                {
                    // The Figure is in Homes or Targets
                    var tileAtStart = GetTileFromBoard(start.Value.x, start.Value.y, _currentGameboard);
                    if (tileAtStart == null || !tileAtStart.IsOccupied)
                        return;

                    if (tileAtStart.Type == TileType.Target)
                    {
                        var figColor = tileAtStart.OccupyingFigure.Color;
                        if (_currentGameboard.Targets != null && _currentGameboard.Targets.TryGetValue(figColor, out var targetArr) && targetArr != null)
                        {
                            int currentIndex = Array.IndexOf(targetArr, tileAtStart);
                            if (currentIndex >= 0)
                            {
                                int desiredIndex = currentIndex + selected.Steps;

                                if (desiredIndex >= 0 && desiredIndex < targetArr.Length)
                                {
                                    var targetTile = targetArr[desiredIndex];
                                    var coord = FindCoordinatesByTileReference(_currentGameboard, targetTile);
                                    if (coord.HasValue)
                                    {
                                        _highlightTarget = coord.Value;
                                    }
                                }
                            }
                        }
                    }
                    else if (tileAtStart.Type == TileType.Home)
                    {
                        var figColor = tileAtStart.OccupyingFigure.Color;
                        for (int i = 0; i < _currentGameboard.Path.Length; i++)
                        {
                            var t = _currentGameboard.Path[i];
                            if (t.Type == TileType.Start && t.Color == figColor)
                            {
                                if (_indexToCoord.TryGetValue(i, out var startCoord))
                                {
                                    _highlightTarget = startCoord;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to find the coordinates of the tile that contains a figure with the given figureIndex.
        /// </summary>
        private (int x, int y)? FindTileCoordinatesForFigureIndex(GameboardDTO board, int figureIndex)
        {
            if (board == null)
                return null;

            // Search Homes
            foreach (var kv in board.Homes)
            {
                var arr = kv.Value;
                if (arr != null)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var t = arr[i];
                        if (t != null && t.IsOccupied && t.OccupyingFigure.Id == figureIndex)
                        {
                            var coord = FindCoordinatesByTileReference(board, t);
                            if (coord.HasValue) return coord;
                        }
                    }
                }
            }

            // search Targets
            foreach (var kv in board.Targets)
            {
                var arr = kv.Value;
                if (arr != null)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var t = arr[i];
                        if (t != null && t.IsOccupied && t.OccupyingFigure.Id == figureIndex)
                        {
                            var coord = FindCoordinatesByTileReference(board, t);
                            if (coord.HasValue) return coord;
                        }
                    }
                }
            }

            // search Path
            if (board.Path != null)
            {
                for (int i = 0; i < board.Path.Length; i++)
                {
                    var t = board.Path[i];
                    if (t != null && t.IsOccupied && t.OccupyingFigure.Id == figureIndex)
                    {
                        if (_indexToCoord.TryGetValue(i, out var coord)) return coord;
                        var coord2 = FindCoordinatesByTileReference(board, t);
                        if (coord2.HasValue) return coord2;
                    }
                }
            }

            return null;
        }

        private (int x, int y)? FindCoordinatesByTileReference(GameboardDTO board, TileDTO tile)
        {
            if (tile == null) return null;
            for (int y = 0; y < 11; y++)
            {
                for (int x = 0; x < 11; x++)
                {
                    var t = GetTileFromBoard(x, y, board);
                    if (ReferenceEquals(t, tile)) return (x, y);
                }
            }
            return null;
        }
    }
}
