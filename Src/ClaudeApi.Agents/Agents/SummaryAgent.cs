using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Agents
{
    public class SummaryAgent(Client client) : Agent
    {
        private readonly Client _client = client ?? throw new ArgumentNullException(nameof(client));

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
            var responses = await _client.ProcessContinuousConversationAsync(prompt, history, systemMessage);

            var resultBuilder = new StringBuilder();

            await foreach (var response in responses)
            {
                if (response != null)
                {
                    resultBuilder.Append(response);
                }
            }

            return resultBuilder.ToString();
        }
    }
}
