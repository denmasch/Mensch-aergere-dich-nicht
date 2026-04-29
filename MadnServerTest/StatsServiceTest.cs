using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MadnServer.Services;
using MadnServer.Player;
using MadnShared.Enums;
using MadnShared.Stats;
using System.Collections.Generic;
using System.Threading.Tasks;
using MadnServerTest.Mocks;
using MadnShared.Messages.Base;

namespace MadnServerTest
{
    [TestClass]
    public class StatsServiceTest
    {
        private Guid _gameId = Guid.NewGuid();

        [TestInitialize]
        public void Init()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var dir = Path.Combine(repoRoot, "MadnServer", "logs", "matches");
            StatsService.Instance.SetOutputDirectory(dir);
            if (Directory.Exists(dir))
            {
                foreach (var f in Directory.GetFiles(dir)) File.Delete(f);
            }
        }

        [TestMethod]
        public void RecordMove_IncrementsMovementCount_And_Captures()
        {
            var p = new MockPlayer { Color = Color.Green };
            var players = new List<IPlayer> { p };
            StatsService.Instance.StartMatch(_gameId, players);

            // single move without capture
            StatsService.Instance.RecordMove(_gameId, p.Id, 1, 4, false, null, DateTime.UtcNow);
            // move with capture
            StatsService.Instance.RecordMove(_gameId, p.Id, 2, 6, true, 99, DateTime.UtcNow);

            // end match to write json
            StatsService.Instance.RecordTurnStart(_gameId, p.Id, DateTime.UtcNow);
            StatsService.Instance.EndMatch(_gameId, DateTime.UtcNow, p.Id, p.Color).Wait();

            var msFile = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")), "MadnServer", "logs", "matches", _gameId + ".json");
            Assert.IsTrue(File.Exists(msFile));

            var json = File.ReadAllText(msFile);
            var stats = System.Text.Json.JsonSerializer.Deserialize<MatchStats>(json);
            Assert.IsNotNull(stats);
            Assert.AreEqual(1, stats.TotalTurns); // one explicit RecordTurnStart in this test
            Assert.AreEqual(GameStatus.Completed, stats.Status);
            Assert.IsTrue(stats.EndTime.HasValue);
            Assert.AreEqual(p.Id, stats.WinnerPlayerId);
            Assert.AreEqual(p.Color, stats.WinnerColor);

            var ps = stats.Players.Find(x => x.PlayerId == p.Id);
            Assert.IsNotNull(ps);
            Assert.AreEqual(10, ps.MovementCount);
            Assert.AreEqual(1, ps.Captures);
        }

        [TestMethod]
        public void CancelMatch_WritesCanceledStatsWithoutWinner()
        {
            var p = new MockPlayer() { Color = Color.Red };
            var players = new List<IPlayer> { p };
            var gameId = Guid.NewGuid();

            StatsService.Instance.StartMatch(gameId, players);
            StatsService.Instance.RecordMove(gameId, p.Id, 1, 3, false, null, DateTime.UtcNow);
            StatsService.Instance.CancelMatch(gameId, DateTime.UtcNow).Wait();

            var msFile = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")), "MadnServer", "logs", "matches", gameId + ".json");
            Assert.IsTrue(File.Exists(msFile));

            var json = File.ReadAllText(msFile);
            var stats = System.Text.Json.JsonSerializer.Deserialize<MatchStats>(json);
            Assert.IsNotNull(stats);
            Assert.AreEqual(GameStatus.Canceled, stats.Status);
            Assert.IsTrue(stats.EndTime.HasValue);
            Assert.IsNull(stats.WinnerPlayerId);
            Assert.IsNull(stats.WinnerColor);

            var ps = stats.Players.Find(x => x.PlayerId == p.Id);
            Assert.IsNotNull(ps);
            Assert.AreEqual(3, ps.MovementCount);
            Assert.AreEqual(0, ps.Captures);
        }
    }
}
