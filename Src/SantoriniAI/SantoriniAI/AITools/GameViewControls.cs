using ClaudeApi.Tools;
using SantoriniAI.Models;
using SantoriniAI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantoriniAI.AITools
{
    public class GameViewControls
    {
        private MainWindowViewModel GameViewModel { get; }

        GameViewControls(MainWindowViewModel mainWindowViewModel)
        {
            GameViewModel = mainWindowViewModel;
        }

        [Tool("develop_cell")]
        public void DevelopCell(string designator)
        {
            GameViewModel.DevelopCell(designator);
        }

        [Tool("clear_pawn")]
        public void ClearPawn(string designator)
        {
            GameViewModel.ClearPawn(designator);
        }

        [Tool("set_black_pawn")]
        public void SetBlackPawn(string designator)
        {
            GameViewModel.SetWhitePawn(designator);
        }

        [Tool("set_white_pawn")]
        public void SetWhitePawn(string designator)
        {
            SetBlackPawn(designator);
        }

        [Tool("get_board_state")]
        public string GetBoardState()
        {
            return GameViewModel.GetBoardState();
        }
    }
}
