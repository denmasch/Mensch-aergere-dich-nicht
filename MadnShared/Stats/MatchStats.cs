using System;
using System.Collections.Generic;
using MadnShared.Enums;

namespace MadnShared.Stats
{
    public class MoveEntry
    {
        public int FigureId { get; set; }
        public int Steps { get; set; }
        public bool Captured { get; set; }
        public int? CapturedFigureId { get; set; }
    }

    public class TurnEntry
    {
        public int TurnNumber { get; set; }
        public Guid PlayerId { get; set; }
        public DateTime Timestamp { get; set; }
        public List<int> DiceRolls { get; set; } = new List<int>();
        public List<MoveEntry> Moves { get; set; } = new List<MoveEntry>();
        public bool Skipped { get; set; }
    }

    public class MatchStats
    {
        public Guid GameId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<PlayerStats> Players { get; set; } = new List<PlayerStats>();
        public List<TurnEntry> Turns { get; set; } = new List<TurnEntry>();
    }
}
