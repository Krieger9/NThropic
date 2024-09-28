using ClaudeApi;
using ClaudeApi.Agents;
using ClaudeApi.Messages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinUI3Host.Core;

namespace WinUI3Host.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IUserInterface
    {
        private string _messageText;
        private readonly StringBuilder _partialMessageBuilder = new();
        private TaskCompletionSource<string> _promptCompletionSource;

        public ObservableCollection<Message> Messages { get; }

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

        public MainViewModel(ClaudeClient apiClient)
        {
            Messages = new ObservableCollection<Message>();
            SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(MessageText);
        }

        private void SendMessage()
        {
            // Complete the TaskCompletionSource with the user input
            _promptCompletionSource?.SetResult(MessageText);
            MessageText = string.Empty; // Clear the input field
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<string> PromptAsync(string message)
        {
            // Create a new TaskCompletionSource to wait for user input
            _promptCompletionSource = new TaskCompletionSource<string>();

            // Wait for the user to click the send button
            string userInput = await _promptCompletionSource.Task;

            // Return the user input
            return userInput;
        }

        public void Message(string message)
        {
            Messages.Add(new Message { Role = "user", Content = [ContentBlock.FromString(message)] });
        }

        public void ReceivePartialMessage(string partialMessage)
        {
            // Append the partial message to the StringBuilder
            _partialMessageBuilder.Append(partialMessage);
        }

        public void EndPartialMessage()
        {
            // Finalize the partial message and add it to the Messages collection
            var completeMessage = _partialMessageBuilder.ToString();
            Messages.Add(new Message { Role = "system", Content =  [ContentBlock.FromString(completeMessage)] });
            _partialMessageBuilder.Clear();
        }
    }
}