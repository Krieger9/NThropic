using System.ComponentModel;
using ClaudeApi.Messages;
using R3;

namespace WinUI3Host.ViewModels
{
    public interface IUsageStatsViewModel : INotifyPropertyChanged
    {
        ReactiveProperty<int> InputTokens { get; }
        ReactiveProperty<int?> OutputTokens { get; set; }
        ReactiveProperty<int?> CacheCreationInputTokens { get; set; }
        ReactiveProperty<int?> CacheReadInputTokens { get; set; }

        void Update(Usage usage);
        void AddUsage(Usage usage);
    }
}
