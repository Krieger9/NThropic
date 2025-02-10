using System;

namespace SantoriniAI.Models
{
    internal enum Pawn
    {
        None,
        Black,
        White
    };

    internal class Cell
    {
        public int DevelopementLevel { get; private set; } = 0;
        public Pawn OccupyingPawn { get; private set; } = Pawn.Black;

        public void Develop()
        {
            if (DevelopementLevel >= 4)
            {
                throw new ApplicationException("Development level cannot exceed 4.");
            }
            DevelopementLevel++;
        }

        public void SetPawn(Pawn pawn)
        {
            OccupyingPawn = pawn;
        }
    }
}
