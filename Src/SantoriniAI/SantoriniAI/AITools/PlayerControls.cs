using SantoriniAI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantoriniAI.AITools
{
    public class PlayerControls
    {
        private MainWindowViewModel GameViewModel { get; set; }

        internal PlayerControls(MainWindowViewModel mainWindowViewModel)
        {
            GameViewModel = mainWindowViewModel;
        }

        public string GetPlayerCellSelection(string selectionPrompt)
        {
            throw new NotImplementedException();
        }
    }
}
