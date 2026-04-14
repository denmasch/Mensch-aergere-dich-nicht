using MadnServer.Gamelogic;
using MadnShared.Enums;

namespace MadnServerTest;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    public void ValidateMove_TargetEntry_OnOwnOccupiedTargetTile_ReturnsFalse()
    {
        Gameboard gb = new();
        Figure movingFigure = new(Color.Yellow, 100);
        Figure ownBlockingFigure = new(Color.Yellow, 101);

        gb.Path[39].OccupyingFigure = movingFigure;
        gb.Targets[Color.Yellow][0].OccupyingFigure = ownBlockingFigure;

        bool isAllowed = MoveValidator.ValidateMove(gb, movingFigure, Color.Yellow, 1);

        Assert.IsFalse(isAllowed);
    }

    [TestMethod]
    public void ValidateMove_TargetEntry_OnFreeTargetTile_ReturnsTrue()
    {
        Gameboard gb = new();
        Figure movingFigure = new(Color.Yellow, 200);

        gb.Path[39].OccupyingFigure = movingFigure;
        gb.Targets[Color.Yellow][0].OccupyingFigure = null;

        bool isAllowed = MoveValidator.ValidateMove(gb, movingFigure, Color.Yellow, 1);

        Assert.IsTrue(isAllowed);
    }

    [TestMethod]
    public void ValidateMove_TargetEntry_OvershootsTargetByOne_ReturnsFalse()
    {
        Gameboard gb = new();
        Figure movingFigure = new(Color.Yellow, 300);

        gb.Path[39].OccupyingFigure = movingFigure;

        bool isAllowed = MoveValidator.ValidateMove(gb, movingFigure, Color.Yellow, 5);

        Assert.IsFalse(isAllowed);
    }
}
