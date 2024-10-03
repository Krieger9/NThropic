using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClaudeApi.Messages;
using R3;

namespace WinUI3Host.ViewModels
{
    public class UsageStatsViewModel : IUsageStatsViewModel
    {
        private readonly Usage _usage;

        public UsageStatsViewModel(Usage usage)
        {
            _usage = usage;
            InputTokens = new ReactiveProperty<int?>(_usage.InputTokens);
            OutputTokens = new ReactiveProperty<int?>(_usage.OutputTokens);
            CacheCreationInputTokens = new ReactiveProperty<int?>(_usage.CacheCreationInputTokens);
            CacheReadInputTokens = new ReactiveProperty<int?>(_usage.CacheReadInputTokens);

            // Subscribe to changes and update the underlying Usage model
            InputTokens.Subscribe(value => _usage.InputTokens = value);
            OutputTokens.Subscribe(value => _usage.OutputTokens = value);
            CacheCreationInputTokens.Subscribe(value => _usage.CacheCreationInputTokens = value);
            CacheReadInputTokens.Subscribe(value => _usage.CacheReadInputTokens = value);
        }

        public ReactiveProperty<int?> InputTokens { get; }
        public ReactiveProperty<int?> OutputTokens { get; }
        public ReactiveProperty<int?> CacheCreationInputTokens { get; }
        public ReactiveProperty<int?> CacheReadInputTokens { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
