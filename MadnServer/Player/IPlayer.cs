using System;
using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnServer.Player;

public interface IPlayer
{
    public Guid Id { get; }
    public Color Color { get; set; }
    
    public Task SendAsync(IMessage message);
}