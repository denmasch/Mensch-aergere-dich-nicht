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
using MadnShared.Logger;

namespace MadnServer.Services
{
    public class StatsService : IStatsService
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<Guid, MatchStats> _activeMatches = new ConcurrentDictionary<Guid, MatchStats>();
        private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new ConcurrentDictionary<Guid, SemaphoreSlim>();
        private string _outDir = Path.Combine("logs", "matches");

        public StatsService(ILogger logger)
        {
            _logger = logger;
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

        // Record a move: increment MovementCount by steps and Captures
        public void RecordMove(Guid gameId, Guid playerId, int figureId, int steps, bool captured, int? capturedFigureId, DateTime time)
        {
            if (!_activeMatches.TryGetValue(gameId, out var ms))
                return;

            var ps = GetPlayerStats(ms, playerId);
            if (ps != null)
            {
                // movement counter counts total tiles moved (sum of steps)
                ps.MovementCount += steps;
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
            await PersistMatch(gameId, endTime, GameStatus: MadnShared.Stats.GameStatus.Completed, winnerPlayerId, winnerColor);
        }

        public async Task CancelMatch(Guid gameId, DateTime endTime)
        {
            await PersistMatch(gameId, endTime, GameStatus: MadnShared.Stats.GameStatus.Canceled);
        }

        private async Task PersistMatch(Guid gameId, DateTime endTime, MadnShared.Stats.GameStatus GameStatus, Guid? winnerPlayerId = null, Color? winnerColor = null)
        {
            if (!_activeMatches.TryRemove(gameId, out var ms))
                return;

            ms.EndTime = endTime;
            ms.Status = GameStatus;

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
                _logger.LogInfo($"Stats written to: {Path.GetFullPath(fname)}");
            }
            catch (Exception ex)
            {
                // log and swallow
                _logger.LogError($"Failed to persist match stats for {gameId}: {ex.Message}");
            }
            finally
            {
                sem.Release();
            }
        }

        // Read all stored match JSON files from the output directory and return deserialized list
        public List<MatchStats> GetStoredMatches()
        {
            var result = new List<MatchStats>();

            try
            {
                if (!Directory.Exists(_outDir))
                    return result;

                var files = Directory.GetFiles(_outDir, "*.json");
                var opts = new JsonSerializerOptions();

                foreach (var f in files)
                {
                    try
                    {
                        var json = File.ReadAllText(f);
                        var ms = JsonSerializer.Deserialize<MatchStats>(json, opts);
                        if (ms != null)
                            result.Add(ms);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to read match file {f}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to enumerate match files in {_outDir}: {ex.Message}");
            }

            return result;
        }
    }
}
