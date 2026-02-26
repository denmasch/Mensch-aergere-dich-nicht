using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnServer.Player;

public interface IPlayer
{
    public Color Color { get; set; }
    
    public Task SendAsync(IGameMessage message);
}