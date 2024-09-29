using ClaudeApi.Messages;
using ClaudeApi.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaudeApi.Agents.Tools;
using System.Collections.ObjectModel;

namespace ClaudeApi.Agents
{
    public partial class ObservableOrchestrationAgent
    {
        private readonly ObservableMessageHistory _messageHistory = new();

        public ObservableMessageHistory MessageHistory => _messageHistory;

        private readonly ClaudeClient _client;
        private readonly IReactiveUserInterface _userInterface;

        public ObservableOrchestrationAgent(ClaudeClient client, IReactiveUserInterface userInterface)
        {
            _client = client;
            _userInterface = userInterface;
            _userInterface.Subscribe(MessageHistory.Messages);
        }

        public async Task StartConversationAsync()
        {
            _client.DiscoverTools(typeof(TestTools).Assembly);
            while (true)
            {
                string userInput = await _userInterface.PromptAsync();
                if (string.IsNullOrEmpty(userInput))
                {
                    break;
                }

                var userMessage = new Message
                {
                    Role = "user",
                    Content = [ContentBlock.FromString(userInput)]
                };

                _messageHistory.AddMessage(userMessage);

                var assistantMessageBuilder = new StringBuilder();

                var streamContentTask = _client.ProcessContinuousConversationAsync(
                    [.. _messageHistory.Messages],
                    systemMessage: [ContentBlock.FromString(SystemPrompt)]);

                await foreach (var streamContent in streamContentTask)
                {
                    assistantMessageBuilder.Append(streamContent);
                }

                var assistantMessage = new Message
                {
                    Role = "assistant",
                    Content = [ContentBlock.FromString(assistantMessageBuilder.ToString())]
                };
                _messageHistory.AddMessage(assistantMessage);
            }
        }
    }
}
