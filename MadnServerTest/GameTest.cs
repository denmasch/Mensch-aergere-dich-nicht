using MadnServer.Gamelogic;
using MadnServer.Player;
using MadnServerTest.Mocks;
using MadnShared.Enums;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;
using MadnShared.Messages.Errors;
using MadnShared.Messages.Base;

namespace MadnServerTest;

[TestClass]
public sealed class GameTest
{
    private static Game CreateGameWithPlayers(out MockPlayer p1, out MockPlayer p2)
    {
        p1 = new MockPlayer();
        p1.Color = Color.Yellow;
        p2 = new MockPlayer();
        p2.Color = Color.Green;
        var players = new List<IPlayer> { p1, p2 };
        return new Game(players);
    }

    [TestMethod]
    public void HandleMessage_StartGameMessage_FromAdmin_StartsGameAndSendsNextPlayer()
    {
        var game = CreateGameWithPlayers(out var p1, out var p2);

        game.HandleMessage(p1, new StartGameMessage { GameId = game.Id });

        Assert.IsTrue(p1.SentMessages.OfType<NextPlayerMessage>().Any());
        Assert.IsTrue(p2.SentMessages.OfType<NextPlayerMessage>().Any());
        var nextMsg = p1.SentMessages.OfType<NextPlayerMessage>().Last();
        Assert.AreEqual(p1.Id, nextMsg.NextPlayerId);
        Assert.AreEqual(p1.Color, nextMsg.NextPlayerColor);
    }

    [TestMethod]
    public void HandleMessage_StartGameMessage_FromNonAdmin_DoesNotStartGame()
    {
        var game = CreateGameWithPlayers(out var p1, out var p2);

        game.HandleMessage(p2, new StartGameMessage { GameId = game.Id });

        Assert.IsFalse(p1.SentMessages.OfType<NextPlayerMessage>().Any());
        Assert.IsFalse(p2.SentMessages.OfType<NextPlayerMessage>().Any());
    }

    [TestMethod]
    public void HandleMessage_RollDiceMessage_FromCurrentPlayer_SendsDiceResult()
    {
        var game = CreateGameWithPlayers(out var p1, out var p2);
        game.HandleMessage(p1, new StartGameMessage { GameId = game.Id });
        p1.SentMessages.Clear();
        p2.SentMessages.Clear();

        game.HandleMessage(p1, new RollDiceMessage { GameId = game.Id });

        var diceMsg = p1.SentMessages.OfType<DiceResultMessage>().Last();
        Assert.AreEqual(game.Id, diceMsg.GameId);
        Assert.IsFalse(p2.SentMessages.OfType<DiceResultMessage>().Any());
    }

    [TestMethod]
    public void HandleMessage_RollDiceMessage_FromNonCurrentPlayer_IsIgnored()
    {
        var game = CreateGameWithPlayers(out var p1, out var p2);
        game.HandleMessage(p1, new StartGameMessage { GameId = game.Id });
        p1.SentMessages.Clear();
        p2.SentMessages.Clear();

        game.HandleMessage(p2, new RollDiceMessage { GameId = game.Id });

        Assert.IsFalse(p1.SentMessages.OfType<DiceResultMessage>().Any());
        Assert.IsFalse(p2.SentMessages.OfType<DiceResultMessage>().Any());
    }

    [TestMethod]
    public void HandleMessage_MoveFigureMessage_FromCurrentPlayer_BroadcastsGameboardAndAdvancesPlayer()
    {
        var game = CreateGameWithPlayers(out var p1, out var p2);
        game.HandleMessage(p1, new StartGameMessage { GameId = game.Id });
        p1.SentMessages.Clear();
        p2.SentMessages.Clear();

        game.HandleMessage(p1, new MoveFigureMessage { GameId = game.Id, PlayerId = p1.Id, FigureId = 1, DiceRoll = 1 });

        Assert.IsTrue(p1.SentMessages.OfType<GameboardUpdatedMessage>().Any());
        Assert.IsTrue(p2.SentMessages.OfType<GameboardUpdatedMessage>().Any());
        Assert.IsTrue(p1.SentMessages.OfType<NextPlayerMessage>().Any());
        Assert.IsTrue(p2.SentMessages.OfType<NextPlayerMessage>().Any());
        var nextMsg = p1.SentMessages.OfType<NextPlayerMessage>().Last();
        Assert.AreEqual(p2.Id, nextMsg.NextPlayerId);
    }

    [TestMethod]
    public void HandleMessage_MoveFigureMessage_FromNonCurrentPlayer_IsIgnored()
    {
        var game = CreateGameWithPlayers(out var p1, out var p2);
        game.HandleMessage(p1, new StartGameMessage { GameId = game.Id });
        p1.SentMessages.Clear();
        p2.SentMessages.Clear();

        game.HandleMessage(p2, new MoveFigureMessage { GameId = game.Id, PlayerId = p2.Id, FigureId = 1, DiceRoll = 1});

        Assert.IsFalse(p1.SentMessages.OfType<GameboardUpdatedMessage>().Any());
        Assert.IsFalse(p2.SentMessages.OfType<GameboardUpdatedMessage>().Any());
        Assert.IsFalse(p1.SentMessages.OfType<NextPlayerMessage>().Any());
        Assert.IsFalse(p2.SentMessages.OfType<NextPlayerMessage>().Any());
    }

    [TestMethod]
    public void HandleMessage_LeaveGameMessage_RemovesPlayerAndBroadcastsGameLeft()
    {
        var game = CreateGameWithPlayers(out var p1, out var p2);

        game.HandleMessage(p1, new LeaveGameMessage { GameId = game.Id , PlayerId = p1.Id});

        Assert.IsFalse(game.Players.Contains(p1));
        Assert.IsTrue(game.Players.Contains(p2));
        var msgP2 = p2.SentMessages.OfType<GameLeftMessage>().Last();
        Assert.AreEqual(game.Id, msgP2.GameId);
        Assert.AreEqual(p1.Id, msgP2.PlayerId);
    }

    [TestMethod]
    public void HandleMessage_UnknownMessageType_BroadcastsUnknownMessageTypeMessage()
    {
        var game = CreateGameWithPlayers(out var p1, out var p2);
    
        game.HandleMessage(p1, new UnknownMessageTypeMessage() { GameId = game.Id });
    
        Assert.IsTrue(p1.SentMessages.OfType<UnknownMessageTypeMessage>().Any());
        Assert.IsTrue(p2.SentMessages.OfType<UnknownMessageTypeMessage>().Any());
        var msg = p1.SentMessages.OfType<UnknownMessageTypeMessage>().Last();
        Assert.AreEqual(game.Id, msg.GameId);
    }

    [TestMethod]
    public void AddPlayer_AssignsNextFreeColor_AndRejectsWhenGameIsFull()
    {
        var p1 = new MockPlayer();
        var p2 = new MockPlayer();
        var game = new Game(new List<IPlayer> { p1, p2 });

        // player 3 and 4 can join 
        var p3 = new MockPlayer();
        var p4 = new MockPlayer();

        var added3 = game.AddPlayer(p3);
        var added4 = game.AddPlayer(p4);

        Assert.IsTrue(added3);
        Assert.IsTrue(added4);
        Assert.AreEqual(4, game.Players.Count);

        // player 5 is rejected because the game is full
        var p5 = new MockPlayer();
        var added5 = game.AddPlayer(p5);

        Assert.IsFalse(added5);
        Assert.AreEqual(4, game.Players.Count);
    }
}
