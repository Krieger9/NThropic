using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents
{
    public class SummaryAgent(ClaudeClient client) : Agent
    {
        private readonly ClaudeClient _client = client ?? throw new ArgumentNullException(nameof(client));

        public override async Task<string> ExecuteAsync(string input, Dictionary<string, object> arguments)
        {
            var prompt = new Prompt("Summarize")
            {
                Arguments = new Dictionary<string, object>
                {
                    { "input", input }
                }
                .Concat(arguments)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            var history = new List<Message>();
            var systemMessage = new List<ContentBlock>();
            var (response,_) = await _client.ProcessContinuousConversationAsync(prompt, history, systemMessage);

            return response;
        }
    }
}
