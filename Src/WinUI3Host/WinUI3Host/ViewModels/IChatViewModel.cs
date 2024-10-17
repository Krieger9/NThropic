using ClaudeApi.Agents;
using ClaudeApi.Messages;
using R3;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinUI3Host.ViewModels
{
    public interface IChatViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Message> Messages { get; }
        ReactiveProperty<string> MessageText { get; set; }
        ReactiveCommand SendMessageCommand { get; }

        Task<string> PromptAsync();
        void Subscribe(ObservableCollection<Message> messages);
    }
}
