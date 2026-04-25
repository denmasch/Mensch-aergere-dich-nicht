using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MadnShared.Stats;
using MadnServer.Gamelogic;
using MadnServer.Player;
using MadnShared.Enums;

namespace MadnServer.Services
{
    public class StatsService
    {
        private static readonly Lazy<StatsService> _instance = new Lazy<StatsService>(() => new StatsService());
        public static StatsService Instance => _instance.Value;

        private readonly ConcurrentDictionary<Guid, MatchStats> _activeMatches = new ConcurrentDictionary<Guid, MatchStats>();
        private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new ConcurrentDictionary<Guid, SemaphoreSlim>();
        private string _outDir = Path.Combine("logs", "matches");

        private StatsService()
        {
            Directory.CreateDirectory(_outDir);
        }

        public void SetOutputDirectory(string outDir)
        {
            if (string.IsNullOrWhiteSpace(outDir)) return;
            _outDir = outDir;
            Directory.CreateDirectory(_outDir);
        }

        public void StartMatch(Guid gameId, IEnumerable<IPlayer> players)
        {
            var ms = new MatchStats
            {
                GameId = gameId,
                StartTime = DateTime.UtcNow,
                TotalTurns = 0
            };

            foreach (var p in players)
            {
                ms.Players.Add(new PlayerStats
                {
                    PlayerId = p.Id,
                    Color = p.Color,
                    MovementCount = 0,
                    Captures = 0
                });
            }

            _activeMatches[gameId] = ms;
            _locks[gameId] = new SemaphoreSlim(1, 1);
        }

        private PlayerStats? GetPlayerStats(MatchStats ms, Guid playerId)
        {
            return ms.Players.Find(p => p.PlayerId == playerId);
        }

        // legacy: record dice roll (no-op for simplified stats)
        public void RecordDiceRoll(Guid gameId, Guid playerId, int diceValue, DateTime time)
        {
            // no longer tracked per requirement
            return;
        }

        // legacy: record unusable dice (no-op)
        public void RecordUnusableDice(Guid gameId, Guid playerId, int diceValue, DateTime time)
        {
            // no longer tracked
            return;
        }

        // Record a move: increment MovementCount and Captures
        public void RecordMove(Guid gameId, Guid playerId, int figureId, int steps, bool captured, int? capturedFigureId, DateTime time)
        {
            if (!_activeMatches.TryGetValue(gameId, out var ms))
                return;

            var ps = GetPlayerStats(ms, playerId);
            if (ps != null)
            {
                // movement counter counts used dice rolls that resulted in moves
                ps.MovementCount++;
                if (captured) ps.Captures++;
            }
        }

        // Count total turns across all players
        public void RecordTurnStart(Guid gameId, Guid playerId, DateTime time)
        {
            if (!_activeMatches.TryGetValue(gameId, out var ms))
                return;

            ms.TotalTurns++;
        }

        public async Task EndMatch(Guid gameId, DateTime endTime, Guid? winnerPlayerId = null, Color? winnerColor = null)
        {
            if (!_activeMatches.TryRemove(gameId, out var ms))
                return;

            ms.EndTime = endTime;

            if (winnerPlayerId.HasValue)
                ms.WinnerPlayerId = winnerPlayerId.Value;
            if (winnerColor.HasValue)
                ms.WinnerColor = winnerColor.Value;

            if (!_locks.TryRemove(gameId, out var sem))
            {
                sem = new SemaphoreSlim(1, 1);
            }

            await sem.WaitAsync();
            try
            {
                var fname = Path.Combine(_outDir, gameId + ".json");
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(ms, opts);
                await File.WriteAllTextAsync(fname, json);
                // log full path for easier debugging
                MadnShared.Logger.Logger.LogInfo($"Stats written to: {Path.GetFullPath(fname)}");
            }
            catch (Exception ex)
            {
                // log and swallow
                MadnShared.Logger.Logger.LogError($"Failed to persist match stats for {gameId}: {ex.Message}");
            }
            finally
            {
                sem.Release();
            }
        }
    }
}
