using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MadnServer.Services;
using MadnServer.Player;
using MadnShared.Enums;
using MadnShared.Stats;
using System.Collections.Generic;
using System.Threading.Tasks;
using MadnShared.Messages.Base;

namespace MadnServerTest
{
    [TestClass]
    public class StatsServiceTest
    {
        private Guid _gameId = Guid.NewGuid();

        private class DummyPlayer : IPlayer
        {
            public Guid Id { get; } = Guid.NewGuid();
            public Color Color { get; set; }
            public Task SendAsync(IMessage message) { return Task.CompletedTask; }
        }

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
            var p = new DummyPlayer { Color = Color.Green };
            var players = new List<IPlayer> { p };
            StatsService.Instance.StartMatch(_gameId, players);

            // single move without capture
            StatsService.Instance.RecordMove(_gameId, p.Id, 1, 4, false, null, DateTime.UtcNow);
            // move with capture
            StatsService.Instance.RecordMove(_gameId, p.Id, 2, 6, true, 99, DateTime.UtcNow);

            // end match to write json
            StatsService.Instance.RecordTurnStart(_gameId, p.Id, DateTime.UtcNow);
            StatsService.Instance.EndMatch(_gameId, DateTime.UtcNow).Wait();

            var msFile = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")), "MadnServer", "logs", "matches", _gameId + ".json");
            Assert.IsTrue(File.Exists(msFile));

            var json = File.ReadAllText(msFile);
            var stats = System.Text.Json.JsonSerializer.Deserialize<MatchStats>(json);
            Assert.IsNotNull(stats);
            Assert.AreEqual(1, stats.TotalTurns); // one explicit RecordTurnStart in this test

            var ps = stats.Players.Find(x => x.PlayerId == p.Id);
            Assert.IsNotNull(ps);
            Assert.AreEqual(2, ps.MovementCount);
            Assert.AreEqual(1, ps.Captures);
        }
    }
}
