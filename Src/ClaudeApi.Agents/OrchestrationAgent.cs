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
    public partial class OrchestrationAgent(Client client, IUserInterface userInterface)
    {
        private readonly MessageHistory _messageHistory = new ();

        public async Task StartConversationAsync()
        {
            client.DiscoverTools(typeof(TestTools).Assembly);
            while (true)
            {
                // Prompt user for input
                string userInput = userInterface.Prompt("You: ");
                if (string.IsNullOrEmpty(userInput))
                {
                    break; // Exit loop if user input is empty
                }

                // Add user message to history
                var userMessage = new Message
                {
                    Role = "user",
                    Content = [ContentBlock.FromString(userInput)]
                };

                _messageHistory.AddMessage(userMessage);

                // Accumulate streaming content
                var assistantMessageBuilder = new StringBuilder();

                // Send messages to LLM and get streaming response
                var streamContentTask = client.ProcessContinuousConversationAsync(
                    _messageHistory.Messages, 
                    systemMessage: [ContentBlock.FromString(SystemPrompt)]);

                await foreach (var streamContent in streamContentTask)
                {
                    // Display streaming content
                    userInterface.ReceivePartialMessage(streamContent);
                    assistantMessageBuilder.Append(streamContent);
                }
                // End partial message display
                userInterface.EndPartialMessage();

                // Add the complete assistant message to history
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
