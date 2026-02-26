using System;
using System.Collections.Generic;
using MadnServer.Player;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;

namespace MadnServer.Gamelogic;

public class Game
{
    public Guid Id { get; }
    public Gameboard Gameboard { get; set; }
    public List<IPlayer> Players { get; private set; }

    private bool _gameStarted = false;
    private int _currentPlayerIndex = 0;

    public Game(Guid id, List<IPlayer> players)
    {
        Id = id;
        Players = players;
        Gameboard = new Gameboard();
    }
    
    public void AddPlayer(IPlayer player)
    {
        Players.Add(player);
    }

    public void StartGame()
    {
        if (_gameStarted || Players.Count == 0)
            return;

        _gameStarted = true;
        _currentPlayerIndex = 0;

        //TODO: start the game
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
                // Player 1 is admin of the goup and can start the game, but only if there is at least 1 player in the game (himself)
                if (!_gameStarted && Players.Count > 0 && Players[0] == fromPlayer)
                {
                    StartGame();
                }
                break;
            case RollDiceMessage rollDice:
                HandleRollDice(fromPlayer, rollDice);
                break;
            case MoveFigureMessage moveFigure:
                HandleMoveFigure(fromPlayer, moveFigure);
                break;
            case LeaveGameMessage leave:
                HandleLeaveGame(fromPlayer, leave);
                break;
            // TODO: handle more message types
        }
    }

    private bool IsCurrentPlayer(IPlayer player)
    {
        if (Players.Count == 0) return false;
        return Players[_currentPlayerIndex] == player;
    }

    private void NextPlayer()
    {
        if (Players.Count == 0) return;
        _currentPlayerIndex = (_currentPlayerIndex + 1) % Players.Count;
        var current = Players[_currentPlayerIndex];

        Broadcast(new NextPlayerMessage
        {
            //TODO: change so that players have an id
            //PlayerId = current.Id
        });
    }

    private void HandleRollDice(IPlayer fromPlayer, RollDiceMessage msg)
    {
        if (!IsCurrentPlayer(fromPlayer))
            return;

        var diceValue = Dice.RollDice();

        //TODO: what if player has no valid move? -> inform player and skip to next player
        
        fromPlayer.SendAsync(new DiceResultMessage
        {
            GameId = Id,
            Value = diceValue
        });
    }

    private void HandleMoveFigure(IPlayer fromPlayer, MoveFigureMessage msg)
    {
        if (!IsCurrentPlayer(fromPlayer))
            return;

        // TODO: Move figure and validate move (update Gameboard)
        Broadcast(new GameboardUpdatedMessage
        {
            GameId = Id,
            Gameboard = Gameboard.ToDto()
        });

        NextPlayer();
    }

    private void HandleLeaveGame(IPlayer fromPlayer, LeaveGameMessage msg)
    {
        Players.Remove(fromPlayer);
        
        if (Players.Count == 0)
        {
            _gameStarted = false;
            GameManager.RemoveGame(Id);
        }

        // Broadcast(new GameLeftMessage
        // {
        //     GameId = Id,
        //     PlayerColor = fromPlayer.Color
        // });
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