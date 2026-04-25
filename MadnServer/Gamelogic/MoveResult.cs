using System;

namespace MadnServer.Gamelogic
{
    public class MoveResult
    {
        public bool Success { get; set; }
        public bool Captured { get; set; }
        public int? CapturedFigureId { get; set; }
    }
}
