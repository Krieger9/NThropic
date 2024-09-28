using ClaudeApi.Messages;
using Newtonsoft.Json;

namespace ClaudeApi.Agents
{
    public class MessageHistory : IMessageHistory
    {
        [JsonProperty("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();

        public void AddMessage(Message message)
        {
            Messages.Add(message);
        }

        public void RemoveMessage(Message message)
        {
            Messages.Remove(message);
        }

        public Message? GetMessageByRole(string role)
        {
            return Messages.FirstOrDefault(m => m.Role == role);
        }
    }
}
