using ClaudeApi.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi
{
    public class MessageHistory
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
