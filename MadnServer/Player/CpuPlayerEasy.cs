using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
using MadnShared.GameAssets;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;

namespace MadnServer.Player;

/// <summary>
/// this player avoids to kick out other players figures.
/// </summary>
public class CpuPlayerEasy : ICpuPlayer
{
    public Color Color { get; set; }
    
    public Guid Id { get; } = Guid.NewGuid();
    
    public async Task SendAsync(IMessage message)
    {
        switch (message)
        {
            case DiceResultMessage diceResult:
                await HandleDiceResultMessage(diceResult);
                break;
            case NextPlayerMessage nextPlayerMessage:
                await HandleNextPlayer(nextPlayerMessage);
                break;
        }
    }

    private async Task HandleNextPlayer(NextPlayerMessage message)
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

    private async Task HandleDiceResultMessage(DiceResultMessage message)
    {
        if (message.PlayerId != Id)
            return;

        if (message.ValidMoves.Count == 0)
            return;
        
        Move bestMove;
        
        // try to find a move that is not a capture
        if (message.ValidMoves.Any(m => !m.IsCapture))
        {
            bestMove = message.ValidMoves.First(m => !m.IsCapture);
        }
        else
        {
            bestMove = message.ValidMoves[Random.Shared.Next(message.ValidMoves.Count)];
        }

        MoveFigureMessage moveFigureMessage = new MoveFigureMessage
        {
            GameId = message.GameId,
            PlayerId = Id,
            FigureId = bestMove.FigureIndex,
            DiceRoll = message.Value
        };
        
        await MessageDispatcher.DispatchAsync(this, moveFigureMessage);
    }
}