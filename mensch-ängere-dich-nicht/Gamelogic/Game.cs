using System.Collections.Generic;
using mensch_ängere_dich_nicht.Player;

namespace mensch_ängere_dich_nicht.Gamelogic;

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