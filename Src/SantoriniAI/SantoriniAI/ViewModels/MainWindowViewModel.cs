using Microsoft.Extensions.DependencyInjection;
using SantoriniAI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantoriniAI.ViewModels
{
    internal class MainWindowViewModel
    {
        public MainWindowViewModel(IServiceProvider serviceProvider)
        {
            Cells = [];

            for (int i = 0; i < 25; i++)
            {
                var cellViewModel = serviceProvider.GetRequiredService<CellViewModel>();
                Cells.Add(cellViewModel);
            }
        }

        public ObservableCollection<CellViewModel> Cells { get; }

        private static int GetCellIndex(string designator)
        {
            if (designator.Length != 2)
                throw new ArgumentException("Invalid designator format. Use format like A1, B2, etc.");

            char columnChar = designator[0];
            char rowChar = designator[1];

            if (columnChar < 'A' || columnChar > 'E' || rowChar < '1' || rowChar > '5')
                throw new ArgumentException("Invalid designator format. Use format like A1, B2, etc.");

            int column = columnChar - 'A';
            int row = '5' - rowChar; // Reverse the row order

            return row * 5 + column;
        }

        public void DevelopCell(string designator)
        {
            int index = GetCellIndex(designator);
            Cells[index].Develop();
        }

        public void SetPawn(string designator, Pawn pawn)
        {
            int index = GetCellIndex(designator);
            Cells[index].SetPawn(pawn);
        }

        public void ClearPawn(string designator)
        {
            int index = GetCellIndex(designator);
            Cells[index].ClearPawn();
        }

        public void SetPawnBlack(string designator)
        {
            int index = GetCellIndex(designator);
            Cells[index].SetPawnBlack();
        }

        public void SetWhitePawn(string designator)
        {
            int index = GetCellIndex(designator);
            Cells[index].SetWhitePawn();
        }

        public string GetDevelopmentLevels()
        {
            StringBuilder sb = new();

            for (int i = 0; i < 25; i++)
            {
                if (i > 0 && i % 5 == 0)
                {
                    sb.AppendLine();
                }
                sb.Append(Cells[i].DevelopmentLevel);
                if (i % 5 != 4)
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }

        public string GetPawnLocations()
        {
            List<string> blackPawns = [];
            List<string> whitePawns = [];

            for (int i = 0; i < 25; i++)
            {
                int row = 5 - (i / 5);
                char column = (char)('A' + (i % 5));
                string location = $"{column}{row}";

                if (Cells[i].GetPawnType() == Pawn.Black)
                {
                    blackPawns.Add(location);
                }
                else if (Cells[i].GetPawnType() == Pawn.White)
                {
                    whitePawns.Add(location);
                }
            }

            return $"Black Pawns: {string.Join(", ", blackPawns)}\nWhite Pawns: {string.Join(", ", whitePawns)}";
        }

        public string GetBoardState()
        {
            StringBuilder sb = new();
            sb.AppendLine("Board State:");
            sb.AppendLine("Development Levels:");
            sb.AppendLine(GetDevelopmentLevels());
            sb.AppendLine();
            sb.AppendLine(GetPawnLocations());
            return sb.ToString();
        }
    }
}
