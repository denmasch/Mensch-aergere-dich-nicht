using System.Collections.Generic;
using MadnServer.Player;

namespace MadnServer.Gamelogic;

public class Game
{
    public Game(List<IPlayer> players)
    {
        Players = players;
    }
    
    public Gameboard Gameboard { get; set; }
    
    public List<IPlayer> Players { get; private set; }

    public void StartGame()
    {
        
    }
}