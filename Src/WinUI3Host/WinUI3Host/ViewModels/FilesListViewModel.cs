using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WinUI3Host.ViewModels
{
    public partial class FilesListViewModel : IFilesListViewModel
    {
        private ObservableCollection<string> _files;

        public ObservableCollection<string> Files
        {
            get { return _files; }
            set
            {
                if (_files != null)
                {
                    _files.CollectionChanged -= OnFilesCollectionChanged;
                }

                _files = value;

                if (_files != null)
                {
                    _files.CollectionChanged += OnFilesCollectionChanged;
                }

                OnPropertyChanged(nameof(Files));
            }
        }

        public FilesListViewModel()
        {
            _files = [];
        }

        public void Subscribe(IObservable<List<string>> filesObservable)
        {
            filesObservable.Subscribe(files =>
            {
                Files = new ObservableCollection<string>(files);
            });
        }

        private void OnFilesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Handle collection changes if needed
            OnPropertyChanged(nameof(Files));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
