using System;
using System.Threading;
using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;

namespace MadnServer.Player;

/// <summary>
/// this player has no stategy and plays random moves
/// </summary>
public class CpuPlayerEasy : ICpuPlayer
{
    public Color Color { get; set; }
    
    public Guid Id { get; } = Guid.NewGuid();
    
    public Task SendAsync(IMessage message)
    {
        switch (message)
        {
            case DiceResultMessage diceResult:
                HandleDiceResultMessage(diceResult);
                break;
            case NextPlayerMessage nextPlayerMessage:
                HandleNextPlayer(nextPlayerMessage);
                break;
        }
        return Task.CompletedTask;
    }

    private async void HandleNextPlayer(NextPlayerMessage message)
    {
        if (message.NextPlayerId != Id)
            return;

        RollDiceMessage rollDiceMessage = new RollDiceMessage
        {
            GameId = message.GameId,
            PlayerId = Id
        };
        
        Thread.Sleep(500);
        
        await MessageDispatcher.DispatchAsync(this, rollDiceMessage);
    }

    private async void HandleDiceResultMessage(DiceResultMessage message)
    {
        if (message.PlayerId != Id)
            return;

        if (message.ValidMoves.Count == 0)
            return;
        
        int randomIndex = Random.Shared.Next(message.ValidMoves.Count);

        MoveFigureMessage moveFigureMessage = new MoveFigureMessage
        {
            GameId = message.GameId,
            PlayerId = Id,
            FigureId = message.ValidMoves[randomIndex].FigureIndex,
            DiceRoll = message.Value
        };
        
        await MessageDispatcher.DispatchAsync(this, moveFigureMessage);
    }
}