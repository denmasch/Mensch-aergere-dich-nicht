using System;
using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnServer.Player;

/// <summary>
/// this player has no stategy and plays random moves
/// </summary>
public class CpuPlayerEasy : ICpuPlayer
{
    public Color Color { get; set; }
    
    public Guid Id { get; } = Guid.NewGuid();
    
    // Stup implemntation for now
    public Task SendAsync(IGameMessage message)
    {
        return Task.CompletedTask;
    }
}