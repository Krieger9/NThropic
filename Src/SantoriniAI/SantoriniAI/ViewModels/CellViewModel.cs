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
        public string? DevelopmentImage
        {
            get { return model.DevelopementLevel > 0 ? $"ms-appx:///Images/BuildingLevel{model.DevelopementLevel}.png" : null; }
        }

        public string BackgroundImage
        {
            get { return $"ms-appx:///Images/BuildingLevel0.png"; }
        }

        public string? PawnImage
        {
            get
            {
                return model.OccupyingPawn switch
                {
                    Pawn.Black => "ms-appx:///Images/BlackPawn.png",
                    Pawn.White => "ms-appx:///Images/WhitePawn.png",
                    _ => null
                };
            }
        }

        public void SetPawn(Pawn pawn)
        {
            model.SetPawn(pawn);
            OnPropertyChanged(nameof(PawnImage));
        }

        public void SetPawnNone()
        {
            SetPawn(Pawn.None);
        }

        public void SetPawnBlack()
        {
            SetPawn(Pawn.Black);
        }

        public void SetPawnWhite()
        {
            SetPawn(Pawn.White);
        }

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
