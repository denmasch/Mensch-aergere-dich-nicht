using System;
using System.Collections.Generic;
using System.Linq;
using MadnServer.Player;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;
using MadnShared.Enums;
using MadnShared.Logger;
using MadnShared.Messages.Errors;

namespace MadnServer.Gamelogic;

public class Game
{
    public Guid Id { get; } = Guid.NewGuid();
    public Gameboard Gameboard { get; set; }
    public List<IPlayer> Players { get; private set; }
    
    public bool IsStarted => _gameStarted;

    private bool _gameStarted = false;
    private int _currentPlayerIndex = 0;

    private const int MaxPlayers = 4;
    private readonly object _colorLock = new object();

    public Game(List<IPlayer> players)
    {
        Players = players;
        lock (_colorLock)
        {
            foreach (var player in players)
            {
                player.Color = GetFirstUnusedColor();
            }
        }
        Gameboard = new Gameboard();
    }
    
    public bool AddPlayer(IPlayer player)
    {
        lock (_colorLock)
        {
            // if game already started or max players reached, reject join
            if (_gameStarted || Players.Count >= MaxPlayers)
            {
                Logger.LogInfo($"Game {Id} is already started or is full. Player cannot join.");
                return false;
            }

            player.Color = GetFirstUnusedColor();
            Players.Add(player);
            Logger.LogInfo($"{player.Color} player joined game {Id}.");
        }
        
        Broadcast(new GameInfoMessage()
        {
            GameId = Id,
            PlayerCount = Players.Count,
            AdminColor = Players[0].Color
        });
        
        return true;
    }

    private Color GetFirstUnusedColor()
    {
        var allColors = Enum.GetValues(typeof(Color)).Cast<Color>().ToList();

        var usedColors = Players.Select(p => p.Color);

        var freeColor = allColors.FirstOrDefault(c => !usedColors.Contains(c));
        return freeColor;
    }

    private void StartGame()
    {
        if (_gameStarted || Players.Count == 0)
            return;

        _gameStarted = true;
        _currentPlayerIndex = 0;

        // First turn
        Broadcast(new NextPlayerMessage
        {
            GameId = Id,
            NextPlayerId = Players[_currentPlayerIndex].Id,
            NextPlayerColor = Players[_currentPlayerIndex].Color
        });
        Logger.LogInfo($"Game {Id} started with {Players.Count} players.");
    }
    
    private bool CheckGameOver()
    {
        var winner = Players.FirstOrDefault(p => Gameboard.IsPlayerWinner(p.Color));
        bool isGameOver = winner != null;
        if (isGameOver)
        {
            Broadcast(new GameOverMessage
            {
                GameId = Id,
                WinnerPlayerId = winner.Id,
                WinnerColor = winner.Color
            });
            
            Logger.LogInfo($"Game {Id} is over. Player {winner.Id} ({winner.Color}) wins!");
            _gameStarted = false;
            GameManager.RemoveGame(Id);
        }

        return isGameOver;
    }

    /// <summary>
    /// Handle Messages from Clients/Players
    /// </summary>
    /// <param name="fromPlayer">The player that sended the message</param>
    /// <param name="message">the message</param>
    public void HandleMessage(IPlayer fromPlayer, IGameMessage message)
    {
        switch (message)
        {
            case StartGameMessage:
                Logger.LogInfo($"Player {fromPlayer.Id} requested to start game {Id}.");
                // Player 1 is admin of the group and can start the game, but only if there is at least 1 player in the game
                if (!_gameStarted && Players.Count > 0 && Players[0] == fromPlayer)
                {
                    StartGame();
                }
                else
                {
                    Logger.LogInfo($"Player {fromPlayer.Id} is not allowed to start game {Id}.");
                }
                break;
            case RollDiceMessage rollDice:
                Logger.LogInfo($"Player {fromPlayer.Id} requested to roll dice in game {Id}.");
                HandleRollDice(fromPlayer, rollDice);
                break;
            case MoveFigureMessage moveFigure:
                Logger.LogInfo($"Player {fromPlayer.Id} requested to move figure in game {Id}.");
                HandleMoveFigure(fromPlayer, moveFigure);
                break;
            case LeaveGameMessage leaveGame:
                Logger.LogInfo($"Player {fromPlayer.Id} requested to leave game {Id}.");
                HandleLeaveGame(fromPlayer, leaveGame);
                break;
            case AddCpuPlayerMessage addCpuPlayer:
                Logger.LogInfo($"Player {fromPlayer.Id} requested to add cpu player {Id}.");
                HandleAddCpuPlayer(fromPlayer, addCpuPlayer);
                break;
            default:
                Logger.LogError($"Unhandled message type {message.GetType().Name}");
                Broadcast(new UnknownMessageTypeMessage
                {
                    GameId = Id
                });
                break;
        }
    }

    private bool IsCurrentPlayer(IPlayer player)
    {
        if (Players.Count == 0) return false;
        return Players[_currentPlayerIndex] == player;
    }

    /// <summary>
    /// Determine the next player and send a NextPlayerMessage to all players in the game
    /// </summary>
    private void NextPlayer()
    {
        if (Players.Count == 0) return;
        
        var current = Players[_currentPlayerIndex];
        
        var colorsCount = Enum.GetValues(typeof(Color)).Length;
        var currentColorIndex = (int)current.Color;
        
        // find next player based on color order (yellow -> green -> red -> blue)
        IPlayer next = null;
        for (int i = 1; i <= colorsCount; i++)
        {
            var candidateIndex = (currentColorIndex + i) % colorsCount;
            var candidateColor = (Color)candidateIndex;

            next = Players.FirstOrDefault(p => p.Color == candidateColor);
            if (next != null)
                break;
        }

        if (next == null)
        {
            next = current;
        }

        var nextIndex = Players.IndexOf(next);
        if (nextIndex >= 0)
            _currentPlayerIndex = nextIndex;

        Broadcast(new NextPlayerMessage
        {
            GameId = Id,
            NextPlayerId = next.Id,
            NextPlayerColor = next.Color
        });
    }
    private void HandleAddCpuPlayer(IPlayer fromPlayer, AddCpuPlayerMessage msg)
    {
        if (_gameStarted || Players.Count >= MaxPlayers || fromPlayer != Players[0])
        {
            Logger.LogInfo($"Player {fromPlayer.Id} is not allowed to add CPU player.");
            return;
        }

        ICpuPlayer cpuPlayer;

        switch (msg.Difficulty)
        {
            case Difficulty.Easy:
                cpuPlayer = new CpuPlayerEasy();
                break;
            case Difficulty.Medium:
                cpuPlayer = new CpuPlayerMedium();
                break;
            case Difficulty.Hard:
                cpuPlayer = new CpuPlayerHard();
                break;
            default:
                Logger.LogError($"Invalid difficulty level {msg.Difficulty} for CPU player.");
                return;
        }
        
        if (AddPlayer(cpuPlayer))
        {
            Logger.LogInfo($"CPU player added to game {Id} by player {fromPlayer.Id}.");
        }
        else
        {
            Logger.LogInfo($"Failed to add CPU player to game {Id}. Game may be full or already started.");
        }
    }

    private void HandleRollDice(IPlayer fromPlayer, RollDiceMessage msg)
    {
        if (!IsCurrentPlayer(fromPlayer))
        {
            Logger.LogInfo($"Player {fromPlayer.Id} attempted to roll dice out of turn in game {Id}.");
            return;
        } 

        var diceValue = Dice.RollDice();
        
        Logger.LogInfo($"Player {fromPlayer.Id} rolled a {diceValue} in game {Id}.");
            
        var validMoves = Gameboard.GetValidMoves(fromPlayer.Color, diceValue);
        
        Logger.LogInfo($"Player {fromPlayer.Id} has {validMoves.Count} valid moves.");
        
        fromPlayer.SendAsync(new DiceResultMessage
        {
            GameId = Id,
            Value = diceValue,
            ValidMoves = validMoves
        });
        
        if (validMoves.Count == 0)
        {
            Logger.LogInfo($"Player {fromPlayer.Id} has no valid moves and will skip their turn in game {Id}.");
            NextPlayer();
        }
    }

    private void HandleMoveFigure(IPlayer fromPlayer, MoveFigureMessage msg)
    {
        if (!IsCurrentPlayer(fromPlayer))
            return;

        var figId = msg.FigureId;
        var col = fromPlayer.Color;

        var fig = Gameboard.GetFigure(col, figId);
        Gameboard.MoveFigure(fig, col, msg.DiceRoll);
        
        Broadcast(new GameboardUpdatedMessage
        {
            GameId = Id,
            Gameboard = Gameboard.ToDto()
        });

        if(CheckGameOver())
            return;

        if (msg.DiceRoll == 6)
        {
            Broadcast(new NextPlayerMessage
            {
                GameId = Id,
                NextPlayerId = fromPlayer.Id,
                NextPlayerColor = fromPlayer.Color
            });
        }
        else
        {
            NextPlayer();
        }
    }
    private void HandleLeaveGame(IPlayer fromPlayer, LeaveGameMessage msg)
    {
        RemovePlayer(fromPlayer);
    }

    public void RemovePlayer(IPlayer fromPlayer)
    {
        var leaveIndex = Players.IndexOf(fromPlayer);
        var leaveColor = fromPlayer.Color;

        Broadcast(new GameLeftMessage
        {
            GameId = Id,
            PlayerId = fromPlayer.Id
        });

        if (leaveIndex < 0)
        {
            Players.Remove(fromPlayer);
            Logger.LogInfo($"Player {fromPlayer.Id} left but was not found in game {Id}.");
            if (Players.Count == 0)
            {
                _gameStarted = false;
                GameManager.RemoveGame(Id);
            }

            return;
        }

        Players.RemoveAt(leaveIndex);

        if (Players.Count == 0)
        {
            _gameStarted = false;
            GameManager.RemoveGame(Id);
            Logger.LogInfo($"Player {fromPlayer.Id} left. No players remaining. Game {Id} closed.");
            return;
        }

        // update current player index if necessary
        if (_gameStarted)
        {
            if (leaveIndex == _currentPlayerIndex)
            {
                var colorsCount = Enum.GetValues(typeof(Color)).Length;
                IPlayer next = null;
                for (int i = 1; i <= colorsCount; i++)
                {
                    var candidateIndex = ((int)leaveColor + i) % colorsCount;
                    var candidateColor = (Color)candidateIndex;

                    next = Players.FirstOrDefault(p => p.Color == candidateColor);
                    if (next != null)
                        break;
                }

                if (next == null)
                {
                    _currentPlayerIndex = 0;
                    next = Players[_currentPlayerIndex];
                }
                else
                {
                    _currentPlayerIndex = Players.IndexOf(next);
                }

                Broadcast(new NextPlayerMessage
                {
                    GameId = Id,
                    NextPlayerId = next.Id,
                    NextPlayerColor = next.Color
                });
            }
            else if (leaveIndex < _currentPlayerIndex)
            {
                // shift current player index down by one since the leaving player was before the current player in the list
                _currentPlayerIndex--;
                if (_currentPlayerIndex < 0) _currentPlayerIndex = 0;
            }

            // if leaveIndex > _currentPlayerIndex, no need to update current player index since the leaving player was after the current player in the list
            Logger.LogInfo($"Player {fromPlayer.Id} left. {Players.Count} players remaining. Current index {_currentPlayerIndex}.");
        }

        Broadcast(new GameInfoMessage()
        {
            GameId = Id,
            PlayerCount = Players.Count,
            AdminColor = Players[0].Color
        });
    }

    /// <summary>
    /// Send Message to alle Players in this Game
    /// </summary>
    /// <param name="msg">Message</param>
    private void Broadcast(IGameMessage msg)
    {
        foreach (var p in Players)
        {
            p.SendAsync(msg);
        }
    }
}