using ClaudeApi.Messages;
using R3;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WinUI3Host.ViewModels
{
    public class ChatViewModel : IChatViewModel
    {
        private TaskCompletionSource<string> _promptCompletionSource;
        private readonly ObservableCollection<Message> _messages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages => _messages;

        private ReactiveProperty<string> _messageText;
        public ReactiveProperty<string> MessageText
        {
            get => _messageText;
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ReactiveCommand SendMessageCommand { get; }

        public ChatViewModel()
        {
            _messageText = new ReactiveProperty<string>();

            SendMessageCommand = _messageText
                .Select(text => !string.IsNullOrWhiteSpace(text))
                .ToReactiveCommand();

            SendMessageCommand.Subscribe(_ => SendMessage());
        }

        private void SendMessage()
        {
            _promptCompletionSource?.SetResult(MessageText.Value);
            MessageText.Value = string.Empty;
            OnPropertyChanged(nameof(MessageText));
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
