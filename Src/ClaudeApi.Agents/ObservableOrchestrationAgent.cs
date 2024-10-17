using ClaudeApi.Messages;
using ClaudeApi.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaudeApi.Agents.Tools;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

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
            _userInterface.Subscribe(_client.UsageStream);
            _userInterface.SubscribeToContextFiles(_client.ContextFilesStream);

            _messageHistory.Messages.CollectionChanged += OnMessagesCollectionChanged;
        }

        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Message newMessage in e.NewItems)
                {
                    newMessage.PropertyChanged += OnMessagePropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Message oldMessage in e.OldItems)
                {
                    oldMessage.PropertyChanged -= OnMessagePropertyChanged;
                }
            }
        }

        private void OnMessagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Notify the UI that a message has changed
            // This can be done by raising a PropertyChanged event or updating the UI directly
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
                    Content = new ObservableCollection<ContentBlock> { ContentBlock.FromString(userInput) }
                };

                _messageHistory.AddMessage(userMessage);

                var streamContentTask = _client.ProcessContinuousConversationAsync(
                    _messageHistory.Messages.ToList(),
                    systemMessage: new List<ContentBlock> { ContentBlock.FromString(SystemPrompt) });

                var userInputContentBlock = ContentBlock.FromString();
                var assistantMessage = new Message
                {
                    Role = "assistant",
                    Content = new ObservableCollection<ContentBlock> { userInputContentBlock }
                };
                _messageHistory.AddMessage(assistantMessage);

                await foreach (var streamContent in streamContentTask)
                {
                    // Update the UI incrementally
                    _userInterface.UpdateContentBlockText(userInputContentBlock, streamContent);
                    await Task.Delay(50);
                }
            }
        }
    }
}
