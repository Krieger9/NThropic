using ClaudeApi;
using ClaudeApi.Agents;
using ClaudeApi.Messages;
using R3;
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
    public class MainViewModel : IChatViewModel, IDisposable
    {
        private readonly ChatViewModel _chatViewModel;
        private readonly IUsageStatsViewModel _lastRequestUsageStats;
        private readonly IUsageStatsViewModel _totalUsageStats;
        private IDisposable _usageSubscription;

        public MainViewModel()
        {
            _chatViewModel = new ChatViewModel();
            _lastRequestUsageStats = new UsageStatsViewModel(new Usage());
            _totalUsageStats = new UsageStatsViewModel(new Usage());
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

        private void UpdateUsageStats(Usage usage)
        {
            // Update last request usage stats
            _lastRequestUsageStats.InputTokens.Value = usage.InputTokens;
            _lastRequestUsageStats.OutputTokens.Value = usage.OutputTokens;
            _lastRequestUsageStats.CacheCreationInputTokens.Value = usage.CacheCreationInputTokens;
            _lastRequestUsageStats.CacheReadInputTokens.Value = usage.CacheReadInputTokens;

            // Accumulate total usage stats
            _totalUsageStats.InputTokens.Value += usage.InputTokens;
            _totalUsageStats.OutputTokens.Value += usage.OutputTokens;
            _totalUsageStats.CacheCreationInputTokens.Value += usage.CacheCreationInputTokens;
            _totalUsageStats.CacheReadInputTokens.Value += usage.CacheReadInputTokens;
        }

        public void Dispose()
        {
            _usageSubscription?.Dispose();
        }
    }
}