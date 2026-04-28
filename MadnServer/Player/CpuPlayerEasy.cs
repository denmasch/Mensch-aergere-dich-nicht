using System;
using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
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

    private void HandleNextPlayer(NextPlayerMessage message)
    {
        if (message.NextPlayerId != Id)
            return;

        RollDiceMessage rollDiceMessage = new RollDiceMessage
        {
            GameId = message.GameId,
            PlayerId = Id
        };
        
        MessageDispatcher.Dispatch(this, rollDiceMessage);
    }

    private void HandleDiceResultMessage(DiceResultMessage message)
    {
        if (message.PlayerId != Id)
            return;

        if (message.ValidMoves.Count == 0)
            return;
        
        var random = new Random();
        int randomIndex = random.Next(message.ValidMoves.Count);

        MoveFigureMessage moveFigureMessage = new MoveFigureMessage
        {
            GameId = message.GameId,
            PlayerId = Id,
            FigureId = message.ValidMoves[randomIndex].FigureIndex,
            DiceRoll = message.Value
        };
        
        MessageDispatcher.Dispatch(this, moveFigureMessage);
    }
}