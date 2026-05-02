using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MadnServer.Player;
using MadnServer.Services;
using MadnShared.Enums;
using MadnShared.Stats;

namespace MadnServerTest.Mocks;

public class MockStatsService : IStatsService
{
    private readonly ConcurrentDictionary<Guid, MatchStats> _activeMatches = new();
    private readonly ConcurrentBag<MatchStats> _storedMatches = new();

    public void SetOutputDirectory(string outDir)
    {
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
    }

    private PlayerStats? GetPlayerStats(MatchStats ms, Guid playerId)
    {
        return ms.Players.Find(p => p.PlayerId == playerId);
    }

    public void RecordMove(Guid gameId, Guid playerId, int figureId, int steps, bool captured, int? capturedFigureId,
        DateTime time)
    {
        if (!_activeMatches.TryGetValue(gameId, out var ms)) return;

        var ps = GetPlayerStats(ms, playerId);
        if (ps != null)
        {
            ps.MovementCount += steps;
            if (captured) ps.Captures++;
        }
    }

    public void RecordTurnStart(Guid gameId, Guid playerId, DateTime time)
    {
        if (!_activeMatches.TryGetValue(gameId, out var ms)) return;
        ms.TotalTurns++;
    }

    public Task EndMatch(Guid gameId, DateTime endTime, Guid? winnerPlayerId = null, Color? winnerColor = null)
    {
        PersistMatch(gameId, endTime, GameStatus.Completed, winnerPlayerId, winnerColor);
        return Task.CompletedTask;
    }

    public Task CancelMatch(Guid gameId, DateTime endTime)
    {
        PersistMatch(gameId, endTime, GameStatus.Canceled, null, null);
        return Task.CompletedTask;
    }

    private void PersistMatch(Guid gameId, DateTime endTime, GameStatus status, Guid? winnerPlayerId, Color? winnerColor)
    {
        if (!_activeMatches.TryRemove(gameId, out var ms)) return;

        ms.EndTime = endTime;
        ms.Status = status;

        if (winnerPlayerId.HasValue) ms.WinnerPlayerId = winnerPlayerId.Value;
        if (winnerColor.HasValue) ms.WinnerColor = winnerColor.Value;

        _storedMatches.Add(ms);
    }

    public List<MatchStats> GetStoredMatches()
    {
        return _storedMatches.ToList();
    }
}