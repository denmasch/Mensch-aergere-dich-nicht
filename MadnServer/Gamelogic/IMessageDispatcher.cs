using System.Threading.Tasks;
using MadnServer.Player;
using MadnShared.Messages.Base;

namespace MadnServer.Gamelogic;

public interface IMessageDispatcher
{
    public Task DispatchAsync(IPlayer fromPlayer, IMessage message);
}