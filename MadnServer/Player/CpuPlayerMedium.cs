using System;
using System.Threading;
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
public class CpuPlayerMedium : ICpuPlayer
{
    public Color Color { get; set; }

    public Guid Id { get; } = Guid.NewGuid();
    
    private readonly Game _game;
    
    public CpuPlayerMedium(Game game)
    {
        _game = game;
    }
    
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
        
        _game.HandleMessage(this, rollDiceMessage);
    }
    
    private async Task HandleDiceResultMessage(DiceResultMessage message)
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
        
        _game.HandleMessage(this, moveFigureMessage);
    }
}