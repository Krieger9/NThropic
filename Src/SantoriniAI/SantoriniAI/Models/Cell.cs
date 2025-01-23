using System;

namespace SantoriniAI.Models
{
    internal class Cell
    {
        public int DevelopementLevel { get; private set; } = 0;

        public void Develop()
        {
            if (DevelopementLevel >= 4)
            {
                throw new ApplicationException("Development level cannot exceed 4.");
            }
            DevelopementLevel++;
        }
    }
}
