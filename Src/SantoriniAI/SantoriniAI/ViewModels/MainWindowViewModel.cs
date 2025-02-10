using Microsoft.Extensions.DependencyInjection;
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
            Cells = new ObservableCollection<CellViewModel>();

            for (int i = 0; i < 25; i++)
            {
                var cellViewModel = serviceProvider.GetRequiredService<CellViewModel>();
                Cells.Add(cellViewModel);
            }
        }

        public ObservableCollection<CellViewModel> Cells { get; }

        internal void DevelopCell(CellViewModel? cell = null)
        {
            // Iterate all entries in Cells and call Develop on each one
            foreach (var c in Cells)
            {
                c.Develop();
            }
        }
    }
}
