using ClaudeApi.Agents;
using ClaudeApi.Messages;
using Microsoft.UI.Dispatching;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace WinUI3Host.ViewModels
{
    public class MainViewModel : IChatViewModel, IDisposable, IReactiveUserInterface
    {
        private readonly ChatViewModel _chatViewModel;
        private readonly IUsageStatsViewModel _lastRequestUsageStats;
        private readonly IUsageStatsViewModel _totalUsageStats;
        private readonly IFilesListViewModel _fileListViewModel;
        private IDisposable _usageSubscription;

        public MainViewModel()
        {
            _chatViewModel = new ChatViewModel();
            _lastRequestUsageStats = new UsageStatsViewModel(new Usage());
            _totalUsageStats = new UsageStatsViewModel(new Usage());
            _fileListViewModel = new FilesListViewModel();
        }

        public ObservableCollection<Message> Messages => _chatViewModel.Messages;

        public ReactiveProperty<string> MessageText
        {
            get => _chatViewModel.MessageText;
            set => _chatViewModel.MessageText = value;
        }

        public ReactiveCommand SendMessageCommand => _chatViewModel.SendMessageCommand;

        public IUsageStatsViewModel LastRequestUsageStats => _lastRequestUsageStats;
        public IUsageStatsViewModel TotalUsageStats => _totalUsageStats;
        public IFilesListViewModel FilesListViewModel => _fileListViewModel;

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

        public void Subscribe(IObservable<Usage> usageStream)
        {
            // Clean up existing subscription if any
            _usageSubscription?.Dispose();
            _usageSubscription = usageStream.Subscribe(UpdateUsageStats);
        }

        public void SubscribeToContextFiles(IObservable<List<string>> file_names)
        {
            _fileListViewModel.Subscribe(file_names);
        }

        private void UpdateUsageStats(Usage usage)
        {
            _lastRequestUsageStats.Update(usage);
            // Accumulate total usage stats
            _totalUsageStats.AddUsage(usage);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _usageSubscription?.Dispose();
        }

        public void UpdateContentBlockText(TextContentBlock userInputContentBlock, string streamContent)
        {
            DispatcherQueue.GetForCurrentThread().TryEnqueue(async () =>
             {
                 userInputContentBlock.Text = string.Concat(userInputContentBlock.Text, streamContent);
             });
        }
    }
}