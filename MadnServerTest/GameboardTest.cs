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
public sealed class GameboardTest
{
    private static Gameboard CreateGameboard()
    {
        return new Gameboard();
    }

    [TestMethod]
    public void GetFigureTest()
    {
        var bd = new Gameboard();
        var color = Color.Green;
        var id = 4711;
        var origFig = new Figure(color, id);
        bd.Path[23].OccupyingFigure = origFig;

        var resFig = bd.GetFigure(color, id);
        
        Assert.AreEqual(origFig, resFig);
    }
    
    [TestMethod]
    public void MoveFigureOutOfHomeTest()
    {
        var gb = new Gameboard();
        var color = Color.Green;
        var diceRollCount = 6;
        var startIndex = gb.GetStartIndexForColor(color);
        var startTile = gb.Path[startIndex];
        var origFig = gb.GetAllFigures(color).First(f => f.IsHome);
        
        var res = gb.MoveFigure(origFig, color, diceRollCount);
        var resFig = startTile.OccupyingFigure;
        
        Assert.IsTrue(res);
        Assert.IsFalse(resFig.IsHome);
        Assert.AreEqual(origFig, resFig);
    }
    
    [TestMethod]
    public void MoveFigureOutOfHomeNo6Test()
    {
        var gb = new Gameboard();
        var color = Color.Green;
        var diceRollCount = 1;
        var startIndex = gb.GetStartIndexForColor(color);
        var startTile = gb.Path[startIndex];
        var origFig = gb.GetAllFigures(color).First(f => f.IsHome);
        
        var res = gb.MoveFigure(origFig, color, diceRollCount);
        var resFig = startTile.OccupyingFigure;
        
        Assert.IsFalse(res);
        Assert.IsNull(resFig);
        Assert.IsTrue(origFig.IsHome);
    }

    [TestMethod]
    public void MoveFigureAlongPathTest()
    {
        var gb = new Gameboard();
        var color = Color.Green;

        // place Figure on the path
        var fig = gb.GetAllFigures(color).First(f => f.IsHome);
        var homeTile = gb.Homes[color].First(t => t.OccupyingFigure == fig);
        homeTile.OccupyingFigure = null;
        fig.IsHome = false;
        int startIndex = gb.GetStartIndexForColor(color);
        gb.Path[startIndex].OccupyingFigure = fig;

        int dice = 3;
        var res = gb.MoveFigure(fig, color, dice);
        
        Assert.IsTrue(res);
        Assert.IsNull(gb.Path[startIndex].OccupyingFigure);
        Assert.AreEqual(fig, gb.Path[(startIndex + dice) % Gameboard.PathLength].OccupyingFigure);
    }

    [TestMethod]
    public void MoveFigureKicksOpponentTest()
    {
        var gb = new Gameboard();
        var attackerColor = Color.Green;
        var defenderColor = Color.Red;

        // place attacker on the path
        var attacker = gb.GetAllFigures(attackerColor).First(f => f.IsHome);
        var atkHome = gb.Homes[attackerColor].First(t => t.OccupyingFigure == attacker);
        atkHome.OccupyingFigure = null;
        attacker.IsHome = false;
        int attackerIndex = 10;
        gb.Path[attackerIndex].OccupyingFigure = attacker;

        // place defender ahead of attacker on the path
        var defender = gb.GetAllFigures(defenderColor).First(f => f.IsHome);
        var defHome = gb.Homes[defenderColor].First(t => t.OccupyingFigure == defender);
        defHome.OccupyingFigure = null;
        defender.IsHome = false;
        int dice = 3;
        int destIndex = (attackerIndex + dice) % Gameboard.PathLength;
        gb.Path[destIndex].OccupyingFigure = defender;

        // attacker kicks the defender
        var res = gb.MoveFigure(attacker, attackerColor, dice);
        
        Assert.IsTrue(res);
        
        // attacker moved forward
        Assert.AreEqual(attacker, gb.Path[destIndex].OccupyingFigure);
        
        // defender got kicked back home 
        Assert.IsTrue(defender.IsHome);
        Assert.IsTrue(gb.Homes[defenderColor].Any(t => t.OccupyingFigure == defender));
    }

    [TestMethod]
    public void MoveFigureIntoTargetTest()
    {
        var gb = new Gameboard();
        var color = Color.Green;

        // place figure on the path
        var fig = gb.GetAllFigures(color).First(f => f.IsHome);
        var homeTile = gb.Homes[color].First(t => t.OccupyingFigure == fig);
        homeTile.OccupyingFigure = null;
        fig.IsHome = false;

        int startIndex = gb.GetStartIndexForColor(color);
        // place figure 1 before target
        int currentIndex = (startIndex + Gameboard.PathLength - 1) % Gameboard.PathLength;
        gb.Path[currentIndex].OccupyingFigure = fig;

        int dice = 1;
        var res = gb.MoveFigure(fig, color, dice);
        
        Assert.IsTrue(res);

        var targetTiles = gb.Targets[color];
        Assert.AreEqual(fig, targetTiles[0].OccupyingFigure);
        Assert.IsNull(gb.Path[currentIndex].OccupyingFigure);
    }

    [TestMethod]
    public void MoveFigureWithinTargetForwardTest()
    {
        var gb = new Gameboard();
        var color = Color.Green;

        var fig = gb.GetAllFigures(color).First(f => f.IsHome);
        var homeTile = gb.Homes[color].First(t => t.OccupyingFigure == fig);
        homeTile.OccupyingFigure = null;
        fig.IsHome = false;

        var targetTiles = gb.Targets[color];
        // set figure directly into target slot 1
        targetTiles[1].OccupyingFigure = fig;

        int dice = 1;
        var res = gb.MoveFigure(fig, color, dice);
        
        Assert.IsTrue(res);

        Assert.IsNull(targetTiles[1].OccupyingFigure);
        Assert.AreEqual(fig, targetTiles[2].OccupyingFigure);
    }
}