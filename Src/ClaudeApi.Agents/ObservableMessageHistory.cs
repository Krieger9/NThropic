using ClaudeApi.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClaudeApi.Agents
{
    public class ObservableMessageHistory : IObservableMessageHistory
    {
        [JsonProperty("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();

        private List<IObserver<Message>> observers = new List<IObserver<Message>>();

        public void AddMessage(Message message)
        {
            Messages.Add(message);
            NotifyObservers(message);
        }

        public void RemoveMessage(Message message)
        {
            Messages.Remove(message);
            // Optionally notify observers about removal
        }

        public Message? GetMessageByRole(string role)
        {
            return Messages.FirstOrDefault(m => m.Role == role);
        }

        public IDisposable Subscribe(IObserver<Message> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
                foreach (var message in Messages)
                {
                    observer.OnNext(message);
                }
            }
            return new Unsubscriber(observers, observer);
        }

        private void NotifyObservers(Message message)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(message);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<Message>> _observers;
            private IObserver<Message> _observer;

            public Unsubscriber(List<IObserver<Message>> observers, IObserver<Message> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}
