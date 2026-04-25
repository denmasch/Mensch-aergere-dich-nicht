// ...existing code...
using System;
using System.Collections.Generic;
using MadnShared.Enums;

namespace MadnShared.Stats
{
    public class PlayerStats
    {
        public Guid PlayerId { get; set; }
        public Color Color { get; set; }
        public int Moves { get; set; }
        public int Captures { get; set; }
        public int DiceRolls { get; set; }
        public int UnusableDice { get; set; }
    }
}
// ...existing code...
