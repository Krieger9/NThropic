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
    public class MainViewModel : IChatViewModel
    {
        private readonly ChatViewModel _chatViewModel;

        public MainViewModel()
        {
            _chatViewModel = new ChatViewModel();
        }

        public ObservableCollection<Message> Messages => _chatViewModel.Messages;

        public string MessageText
        {
            get => _chatViewModel.MessageText;
            set => _chatViewModel.MessageText = value;
        }

        public ICommand SendMessageCommand => _chatViewModel.SendMessageCommand;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _chatViewModel.PropertyChanged += value;
            remove => _chatViewModel.PropertyChanged -= value;
        }

        public async Task<string> PromptAsync()
        {
            return await _chatViewModel.PromptAsync();
        }

        public void Subscribe(ObservableCollection<Message> messages)
        {
            _chatViewModel.Subscribe(messages);
        }
    }
}