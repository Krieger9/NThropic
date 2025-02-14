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
    public partial class MainViewModel(
            IChatViewModel chatViewModel,
            IUsageStatsViewModel lastRequestUsageStats,
            IUsageStatsViewModel totalUsageStats,
            IFilesListViewModel fileListViewModel
        ) : IChatViewModel, IDisposable, IReactiveUserInterface
    {
        private readonly IChatViewModel _chatViewModel = chatViewModel;
        private readonly IUsageStatsViewModel _lastRequestUsageStats = lastRequestUsageStats;
        private readonly IUsageStatsViewModel _totalUsageStats = totalUsageStats;
        private readonly IFilesListViewModel _fileListViewModel = fileListViewModel;
        private IDisposable? _usageSubscription;

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

        public event PropertyChangedEventHandler? PropertyChanged
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
            DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                userInputContentBlock.Text = string.Concat(userInputContentBlock.Text, streamContent);
            });
        }
    }
}