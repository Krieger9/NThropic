using ClaudeApi.Messages;
using System.Collections.Generic;

namespace ClaudeApi.Agents
{
    public interface IMessageHistory
    {
        List<Message> Messages { get; set; }
        void AddMessage(Message message);
        void RemoveMessage(Message message);
        Message? GetMessageByRole(string role);
    }
}
