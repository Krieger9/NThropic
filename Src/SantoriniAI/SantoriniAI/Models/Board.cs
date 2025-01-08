using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantoriniAI.Models
{
    namespace SantoriniAI.Models
    {
        internal class Board
        {
            private readonly Cell[,] grid;

            public Board(IConfiguration configuration)
            {
                int gridWidth = configuration.GetValue<int>("Grid:Width");
                int gridHeight = configuration.GetValue<int>("Grid:Height");
                grid = new Cell[gridWidth, gridHeight];
                InitializeGrid();
            }

            private void InitializeGrid()
            {
                for (int i = 0; i < grid.GetLength(0); i++)
                {
                    for (int j = 0; j < grid.GetLength(1); j++)
                    {
                        grid[i, j] = new Cell();
                    }
                }
            }
        }
    }
}
