using ClaudeApi.Messages;
using Microsoft.UI.Dispatching;
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
        private TaskCompletionSource<string>? _promptCompletionSource;
        private readonly ObservableCollection<Message> _messages = [];
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

            _messages.CollectionChanged += OnMessagesCollectionChanged;
        }

        private void SendMessage()
        {
            _promptCompletionSource?.SetResult(MessageText.Value);
            MessageText.Value = string.Empty;
            OnPropertyChanged(nameof(MessageText));
        }

        public static void UpdateContentBlockText(TextContentBlock contentBlock, string newText)
        {
            DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                contentBlock.Text = newText;
            });
        }

        private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Message newMessage in e.NewItems)
                {
                    newMessage.PropertyChanged += OnMessagePropertyChanged;
                    foreach (var contentBlock in newMessage.Content)
                    {
                        if (contentBlock is INotifyPropertyChanged notifyPropertyChanged)
                        {
                            notifyPropertyChanged.PropertyChanged += OnContentBlockPropertyChanged;
                        }
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (Message oldMessage in e.OldItems)
                {
                    oldMessage.PropertyChanged -= OnMessagePropertyChanged;
                    foreach (var contentBlock in oldMessage.Content)
                    {
                        if (contentBlock is INotifyPropertyChanged notifyPropertyChanged)
                        {
                            notifyPropertyChanged.PropertyChanged -= OnContentBlockPropertyChanged;
                        }
                    }
                }
            }
        }

        private void OnMessagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Messages));
        }

        private void OnContentBlockPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Messages));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

        private void OnModelMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
    }
}
