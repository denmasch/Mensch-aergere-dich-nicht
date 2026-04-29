using System;
using System.Collections.Generic;
using MadnShared.Enums;

namespace MadnShared.Stats
{
    public enum GameStatus
    {
        Completed,  // Game finished normally with a winner
        Canceled    // Game was canceled (all player left)
    }

    public class MatchStats
    {
        public Guid GameId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<PlayerStats> Players { get; set; } = new List<PlayerStats>();
        // total number of turns across all players
        public int TotalTurns { get; set; }

        // winner info (nullable if no winner / draw)
        public Guid? WinnerPlayerId { get; set; }
        public Color? WinnerColor { get; set; }
        
        // Game status: Completed or Canceled
        public GameStatus Status { get; set; } = GameStatus.Completed;
    }
}
