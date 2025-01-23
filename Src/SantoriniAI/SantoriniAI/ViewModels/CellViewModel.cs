using SantoriniAI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantoriniAI.ViewModels
{
    internal partial class CellViewModel(Cell model) : INotifyPropertyChanged
    {
        public string? DevelopmentImage { get { return model.DevelopementLevel > 0 ? $"ms-appx:///Images/BuildingLevel{model.DevelopementLevel}.png" : null; } }
        public string BackgroundImage { get { return $"ms-appx:///Images/BuildingLevel0.png"; } }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Develop()
        {
            model.Develop();
            OnPropertyChanged(nameof(DevelopmentImage));
        }
    }
}
