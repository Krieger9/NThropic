using ClaudeApi.Messages;
using System;
using System.Collections.Generic;

namespace ClaudeApi.Agents
{
    public interface IObservableMessageHistory : IMessageHistory, IObservable<Message>
    {
    }
}
