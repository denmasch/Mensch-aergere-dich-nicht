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
        private readonly string _outDir = "logs/matches";

        private StatsService()
        {
            Directory.CreateDirectory(_outDir);
        }

        public void StartMatch(Guid gameId, IEnumerable<IPlayer> players)
        {
            var ms = new MatchStats
            {
                GameId = gameId,
                StartTime = DateTime.UtcNow
            };

            foreach (var p in players)
            {
                ms.Players.Add(new PlayerStats
                {
                    PlayerId = p.Id,
                    Color = p.Color,
                    Moves = 0,
                    Captures = 0,
                    DiceRolls = 0,
                    UnusableDice = 0
                });
            }

            _activeMatches[gameId] = ms;
            _locks[gameId] = new SemaphoreSlim(1, 1);
        }

        private PlayerStats? GetPlayerStats(MatchStats ms, Guid playerId)
        {
            return ms.Players.Find(p => p.PlayerId == playerId);
        }

        public void RecordDiceRoll(Guid gameId, Guid playerId, int diceValue, DateTime time)
        {
            if (!_activeMatches.TryGetValue(gameId, out var ms))
                return;

            // Append dice roll to the last turn if it belongs to the same player, otherwise create a new turn
            var lastTurn = ms.Turns.Count > 0 ? ms.Turns[^1] : null;
            if (lastTurn != null && lastTurn.PlayerId == playerId)
            {
                lastTurn.DiceRolls.Add(diceValue);
            }
            else
            {
                var turn = new TurnEntry { TurnNumber = ms.Turns.Count + 1, PlayerId = playerId, Timestamp = time };
                turn.DiceRolls.Add(diceValue);
                ms.Turns.Add(turn);
            }

            var ps = GetPlayerStats(ms, playerId);
            if (ps != null)
                ps.DiceRolls++;
        }

        public void RecordUnusableDice(Guid gameId, Guid playerId, int diceValue, DateTime time)
        {
            if (!_activeMatches.TryGetValue(gameId, out var ms))
                return;

            // attach unusable dice to last turn if same player
            var lastTurn = ms.Turns.Count > 0 ? ms.Turns[^1] : null;
            if (lastTurn != null && lastTurn.PlayerId == playerId)
            {
                lastTurn.DiceRolls.Add(diceValue);
                lastTurn.Skipped = true;
            }
            else
            {
                var turn = new TurnEntry { TurnNumber = ms.Turns.Count + 1, PlayerId = playerId, Timestamp = time, Skipped = true };
                turn.DiceRolls.Add(diceValue);
                ms.Turns.Add(turn);
            }

            var ps = GetPlayerStats(ms, playerId);
            if (ps != null)
                ps.UnusableDice++;
        }

        public void RecordMove(Guid gameId, Guid playerId, int figureId, int steps, bool captured, int? capturedFigureId, DateTime time)
        {
            if (!_activeMatches.TryGetValue(gameId, out var ms))
                return;

            // attach to last turn if same player
            var lastTurn = ms.Turns.Count > 0 ? ms.Turns[^1] : null;
            if (lastTurn == null || lastTurn.PlayerId != playerId)
            {
                lastTurn = new TurnEntry { TurnNumber = ms.Turns.Count + 1, PlayerId = playerId, Timestamp = time };
                ms.Turns.Add(lastTurn);
            }

            lastTurn.Moves.Add(new MoveEntry { FigureId = figureId, Steps = steps, Captured = captured, CapturedFigureId = capturedFigureId });

            var ps = GetPlayerStats(ms, playerId);
            if (ps != null)
            {
                ps.Moves++;
                if (captured) ps.Captures++;
            }
        }

        public void RecordTurnStart(Guid gameId, Guid playerId, DateTime time)
        {
            if (!_activeMatches.TryGetValue(gameId, out var ms))
                return;

            // always create a new TurnEntry with monotonically increasing TurnNumber
            var turn = new TurnEntry { TurnNumber = ms.Turns.Count + 1, PlayerId = playerId, Timestamp = time };
            ms.Turns.Add(turn);
        }

        public async Task EndMatch(Guid gameId, DateTime endTime)
        {
            if (!_activeMatches.TryRemove(gameId, out var ms))
                return;

            ms.EndTime = endTime;

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
