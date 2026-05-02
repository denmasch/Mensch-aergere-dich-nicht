using System;
using System.Collections.Generic;
using MadnServer.Player;

namespace MadnServer.Gamelogic;

public interface IGameManager
{
    public Game CreateGame(IPlayer player);
    public Game? GetGame(Guid gameId);
    public Dictionary<Guid, int> GetAllJoinableGames();
    public void RemoveGame(Guid gameId);
    public Game TryJoinGame(Guid gameId, IPlayer player);
    public void RemovePlayerFromGames(IPlayer player);  
}