using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MadnServer.Gamelogic;
using MadnShared.Enums;
using MadnShared.GameAssets;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;

namespace MadnServer.Player;

public abstract class CpuPlayer : ICpuPlayer
{
    public Color Color { get; set; }
    public Guid Id { get; } = Guid.NewGuid();
    
    private readonly Game _game;

    protected CpuPlayer(Game game)
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
            case NextPlayerMessage nextPlayer:
                await HandleNextPlayer(nextPlayer);
                break;
        }
    }

    private async Task HandleNextPlayer(NextPlayerMessage message)
    {
        if (message.NextPlayerId != Id)
            return;

        await Task.Delay(500);

        var rollDice = new RollDiceMessage
        {
            GameId = message.GameId,
            PlayerId = Id
        };

        _game.HandleMessage(this, rollDice);
    }

    private async Task HandleDiceResultMessage(DiceResultMessage message)
    {
        if (message.PlayerId != Id || message.ValidMoves.Count == 0)
            return;

        var selectedMove = SelectMove(message.ValidMoves);

        var move = new MoveFigureMessage
        {
            GameId = message.GameId,
            PlayerId = Id,
            FigureId = selectedMove.FigureIndex,
            DiceRoll = message.Value
        };

        _game.HandleMessage(this, move);
    }

    protected abstract Move SelectMove(IReadOnlyList<Move> validMoves);
}