using System;
using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
using MadnShared.Messages.Base;

namespace MadnServer.Player;

/// <summary>
/// this player tries really hard to kick other players figures out
/// </summary>
public class CpuPlayerHard : ICpuPlayer
{
    public Color Color { get; set; }
    
    public Guid Id { get; } = Guid.NewGuid();
    // Stup implemntation for now
    public Task SendAsync(IGameMessage message)
    {
        return Task.CompletedTask;
    }
}