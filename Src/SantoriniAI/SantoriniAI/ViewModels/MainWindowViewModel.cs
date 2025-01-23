using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantoriniAI.ViewModels
{
    internal class MainWindowViewModel
    {
        public MainWindowViewModel(CellViewModel cell)
        {
            Cell = cell;
        }

        public CellViewModel Cell { get; }

        internal void DevelopCell()
        {
            Cell.Develop();
        }
    }
}
