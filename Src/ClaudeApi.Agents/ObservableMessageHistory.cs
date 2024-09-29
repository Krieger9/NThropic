using ClaudeApi.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents
{
    public class ObservableMessageHistory : IObservableMessageHistory, IDisposable
    {
        private readonly Subject<Message> _messageSubject = new();
        private readonly ObservableCollection<Message> _messages = new();

        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set
            {
                _messages.Clear();
                foreach (var message in value)
                {
                    _messages.Add(message);
                }
            }
        }

        public IDisposable Subscribe(IObserver<Message> observer)
        {
            return _messageSubject.Subscribe(observer);
        }

        public void AddMessage(Message message)
        {
            _messages.Add(message);
            _messageSubject.OnNext(message);
        }

        public void RemoveMessage(Message message)
        {
            _messages.Remove(message);
        }

        public Message? GetMessageByRole(string role)
        {
            return _messages.FirstOrDefault(m => m.Role == role);
        }

        public void Dispose()
        {
            _messageSubject.Dispose();
        }
    }
}
