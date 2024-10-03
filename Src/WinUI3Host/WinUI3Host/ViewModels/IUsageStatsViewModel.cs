using System.ComponentModel;
using R3;

namespace WinUI3Host.ViewModels
{
    public interface IUsageStatsViewModel : INotifyPropertyChanged
    {
        ReactiveProperty<int?> InputTokens { get; }
        ReactiveProperty<int?> OutputTokens { get; }
        ReactiveProperty<int?> CacheCreationInputTokens { get; }
        ReactiveProperty<int?> CacheReadInputTokens { get; }
    }
}
