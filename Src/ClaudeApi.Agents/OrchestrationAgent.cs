using ClaudeApi.Messages;
using ClaudeApi.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaudeApi.Agents.Tools;

namespace ClaudeApi.Agents
{
    public partial class OrchestrationAgent
    {
        private readonly IMessageHistory _messageHistory;
        private readonly IUserInterface _userInterface;
        private readonly ClaudeClient _client;

        public OrchestrationAgent(ClaudeClient client, IUserInterface userInterface, IMessageHistory messageHistory)
        {
            _messageHistory = messageHistory;
            _userInterface = userInterface;
            _client = client;
        }

        public IMessageHistory MessageHistory => _messageHistory;

        public event Action<Message>? MessageAdded;

        private void OnMessageAdded(Message message)
        {
            MessageAdded?.Invoke(message);
        }

        public async Task StartConversationAsync()
        {
            _client.DiscoverTools(typeof(TestTools).Assembly);
            while (true)
            {
                string userInput = await _userInterface.PromptAsync("You: ");
                if (string.IsNullOrEmpty(userInput))
                {
                    break;
                }

                var userMessage = new Message
                {
                    Role = "user",
                    Content = new List<ContentBlock> { ContentBlock.FromString(userInput) }
                };

                _messageHistory.AddMessage(userMessage);
                OnMessageAdded(userMessage);

                var assistantMessageBuilder = new StringBuilder();
                var streamContentTask = _client.ProcessContinuousConversationAsync(
                    _messageHistory.Messages,
                    systemMessage: new List<ContentBlock> { ContentBlock.FromString(SystemPrompt) });

                await foreach (var streamContent in streamContentTask)
                {
                    _userInterface.ReceivePartialMessage(streamContent);
                    assistantMessageBuilder.Append(streamContent);
                }

                _userInterface.EndPartialMessage();

                var assistantMessage = new Message
                {
                    Role = "assistant",
                    Content = new List<ContentBlock> { ContentBlock.FromString(assistantMessageBuilder.ToString()) }
                };

                _messageHistory.AddMessage(assistantMessage);
                OnMessageAdded(assistantMessage);
            }
        }
    }
}
