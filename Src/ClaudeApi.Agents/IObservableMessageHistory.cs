using ClaudeApi.Messages;
using System.Collections.ObjectModel;

namespace ClaudeApi.Agents
{
    public interface IObservableMessageHistory
    {
        ObservableCollection<Message> Messages { get; set; }
        IDisposable Subscribe(IObserver<Message> observer);
        void AddMessage(Message message);
        void RemoveMessage(Message message);
        Message? GetMessageByRole(string role);
    }
}
