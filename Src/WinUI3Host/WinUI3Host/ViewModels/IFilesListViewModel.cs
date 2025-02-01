using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

public interface IFilesListViewModel : INotifyPropertyChanged
{
    ObservableCollection<string>? Files { get; set; }
    void Subscribe(IObservable<List<string>> fies);
}
