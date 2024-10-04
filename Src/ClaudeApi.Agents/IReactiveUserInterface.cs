using ClaudeApi.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents
{
    public interface IReactiveUserInterface
    {
        Task<string> PromptAsync();
        void Subscribe(ObservableCollection<Message> messages);
        void Subscribe(IObservable<Usage> usageStream);
        void SubscribeToContextFiles(IObservable<List<string>> contextFilesStream);
    }
}
