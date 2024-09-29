using ClaudeApi;
using ClaudeApi.Agents;
using ClaudeApi.Messages;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinUI3Host.Core;

namespace WinUI3Host.ViewModels
{
    public class MainViewModel : IReactiveUserInterface
    {
        private string _messageText;
        private TaskCompletionSource<string> _promptCompletionSource;
        private readonly ObservableCollection<Message> _messages = [];
        public ObservableCollection<Message> Messages => _messages;

        public string MessageText
        {
            get => _messageText;
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged();
                    ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SendMessageCommand { get; }

        public MainViewModel()
        {
            SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(MessageText);
        }

        private void SendMessage()
        {
            _promptCompletionSource?.SetResult(MessageText);
            MessageText = string.Empty;
        }

        private void OnModelMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Message newMessage in e.NewItems)
                {
                    _messages.Add(newMessage);
                }
            }
            if (e.OldItems != null)
            {
                foreach (Message oldMessage in e.OldItems)
                {
                    _messages.Remove(oldMessage);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<string> PromptAsync()
        {
            _promptCompletionSource = new TaskCompletionSource<string>();
            return await _promptCompletionSource.Task;
        }

        public void Subscribe(ObservableCollection<Message> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);

            messages.CollectionChanged += OnModelMessagesChanged;

            foreach (var message in messages)
            {
                _messages.Add(message);
            }
        }
    }
}