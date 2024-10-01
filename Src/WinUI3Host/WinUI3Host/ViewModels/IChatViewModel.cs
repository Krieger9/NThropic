using ClaudeApi.Agents;
using ClaudeApi.Messages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace WinUI3Host.ViewModels
{
    public interface IChatViewModel : IReactiveUserInterface, INotifyPropertyChanged
    {
        ObservableCollection<Message> Messages { get; }
        string MessageText { get; set; }
        ICommand SendMessageCommand { get; }
    }
}
