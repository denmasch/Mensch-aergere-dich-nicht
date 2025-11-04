using System.Collections.Generic;
using MadnServer.Player;

namespace MadnServer.Gamelogic;

public class Game
{
    public Game(List<IPlayer> players)
    {
        Players = players;
        Gameboard = new Gameboard();
    }
    
    public Gameboard Gameboard { get; set; }
    
    public List<IPlayer> Players { get; private set; }
    
    private bool _gameStarted = false;

    public void StartGame()
    {
        _gameStarted = true;
        while (_gameStarted)
        {
            foreach (var player in Players)
            {
                
                
            }
        }
    }
}