using System;
using MadnShared.Enums;

namespace MadnShared.Stats
{
    public class PlayerStats
    {
        public Guid PlayerId { get; set; }
        public Color Color { get; set; }
        // total used dice rolls that resulted in moves (movement counter)
        public int MovementCount { get; set; }
        // number of opponent figures this player captured
        public int Captures { get; set; }
    }
}
