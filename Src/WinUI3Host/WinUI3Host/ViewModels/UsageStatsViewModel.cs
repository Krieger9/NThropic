using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClaudeApi.Messages;
using R3;

namespace WinUI3Host.ViewModels
{
    public class UsageStatsViewModel : IUsageStatsViewModel
    {
        private ReactiveProperty<int> _inputTokens;
        private ReactiveProperty<int?> _outputTokens;
        private ReactiveProperty<int?> _cacheCreationInputTokens;
        private ReactiveProperty<int?> _cacheReadInputTokens;

        public ReactiveProperty<int> InputTokens
        {
            get => _inputTokens;
            set
            {
                if (_inputTokens != value)
                {
                    _inputTokens = value;
                    OnPropertyChanged();
                }
            }
        }

        public ReactiveProperty<int?> OutputTokens
        {
            get => _outputTokens;
            set
            {
                if (_outputTokens != value)
                {
                    _outputTokens = value;
                    OnPropertyChanged();
                }
            }
        }

        public ReactiveProperty<int?> CacheCreationInputTokens
        {
            get => _cacheCreationInputTokens;
            set
            {
                if (_cacheCreationInputTokens != value)
                {
                    _cacheCreationInputTokens = value;
                    OnPropertyChanged();
                }
            }
        }

        public ReactiveProperty<int?> CacheReadInputTokens
        {
            get => _cacheReadInputTokens;
            set
            {
                if (_cacheReadInputTokens != value)
                {
                    _cacheReadInputTokens = value;
                    OnPropertyChanged();
                }
            }
        }

        public UsageStatsViewModel(Usage usage)
        {
            _inputTokens = new ReactiveProperty<int>(usage.InputTokens);
            _outputTokens = new ReactiveProperty<int?>(usage.OutputTokens);
            _cacheCreationInputTokens = new ReactiveProperty<int?>(usage.CacheCreationInputTokens);
            _cacheReadInputTokens = new ReactiveProperty<int?>(usage.CacheReadInputTokens);
        }

        public void Update(Usage usage)
        {
            InputTokens.Value = usage.InputTokens;
            OutputTokens.Value = usage.OutputTokens;
            CacheCreationInputTokens.Value = usage.CacheCreationInputTokens;
            CacheReadInputTokens.Value = usage.CacheReadInputTokens;

            OnPropertyChanged(nameof(InputTokens));
            OnPropertyChanged(nameof(OutputTokens));
            OnPropertyChanged(nameof(CacheCreationInputTokens));
            OnPropertyChanged(nameof(CacheReadInputTokens));
        }

        public void AddUsage(Usage usage)
        {
            InputTokens.Value += usage.InputTokens;
            OutputTokens.Value += usage.OutputTokens;
            CacheCreationInputTokens.Value += usage.CacheCreationInputTokens;
            CacheReadInputTokens.Value += usage.CacheReadInputTokens;

            OnPropertyChanged(nameof(InputTokens));
            OnPropertyChanged(nameof(OutputTokens));
            OnPropertyChanged(nameof(CacheCreationInputTokens));
            OnPropertyChanged(nameof(CacheReadInputTokens));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
