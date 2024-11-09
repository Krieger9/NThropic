using ClaudeApi.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Prompts
{
    public class PromptResponse
    {
        [JsonProperty("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();

        [JsonProperty("meta")]
        public Dictionary<string, string>? Meta { get; set; } = new Dictionary<string, string>();
    }
}
