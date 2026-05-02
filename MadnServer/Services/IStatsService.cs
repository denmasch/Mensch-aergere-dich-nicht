using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MadnServer.Player;
using MadnShared.Enums;
using MadnShared.Stats;

namespace MadnServer.Services;

public interface IStatsService
{
    public void SetOutputDirectory(string outDir);
    
    public void StartMatch(Guid gameId, IEnumerable<IPlayer> players);

    public void RecordMove(Guid gameId, Guid playerId, int figureId, int steps, bool captured, int? capturedFigureId, DateTime time);

    public void RecordTurnStart(Guid gameId, Guid playerId, DateTime time);

    public Task EndMatch(Guid gameId, DateTime endTime, Guid? winnerPlayerId = null, Color? winnerColor = null);
    
    public Task CancelMatch(Guid gameId, DateTime endTime);
    
    public List<MatchStats> GetStoredMatches();
}
