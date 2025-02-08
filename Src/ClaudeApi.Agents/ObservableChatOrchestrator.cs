using ClaudeApi.Agents.ChatTracking;
using ClaudeApi.Agents.ChatTracking.OpenTelemetry;
using ClaudeApi.Agents.Tools;
using ClaudeApi.Messages;
using System.Collections.Specialized;
using System.ComponentModel;
using ClaudeApi.Agents.Agents;
using System.Text;

namespace ClaudeApi.Agents
{
    public partial class ObservableChatOrchestrator
    {
        private readonly ObservableMessageHistory _messageHistory = new();
        private readonly IConversationLogger _conversationLogger;

        public ObservableMessageHistory MessageHistory => _messageHistory;

        private readonly ClaudeClient _client;
        private readonly IReactiveUserInterface _userInterface;
        private readonly ContextualizeAgent _contextualizeAgent;

        public ObservableChatOrchestrator(ClaudeClient client, IReactiveUserInterface userInterface, IConversationLogger conversationLogger, IContextualizeAgent contextualizeAgent)
        {
            _client = client;
            _userInterface = userInterface;
            _conversationLogger = conversationLogger;
            _userInterface.Subscribe(MessageHistory.Messages);
            _userInterface.Subscribe(_client.UsageStream);
            _userInterface.SubscribeToContextFiles(_client.ContextFilesStream);
            _contextualizeAgent = contextualizeAgent;

            _messageHistory.Messages.CollectionChanged += OnMessagesCollectionChanged;
        }

        private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

        private void OnMessagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Notify the UI that a message has changed
            // This can be done by raising a PropertyChanged event or updating the UI directly
        }

        public async Task StartConversationAsync()
        {
            _client.DiscoverTools(typeof(TestTools).Assembly);
            var conversation = new Conversation();
            _conversationLogger.SaveConversation(conversation);

            try
            {
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

                    // Log user message
                    _conversationLogger.LogMessage(new MessageLogEntry
                    {
                        ConversationId = conversation.ConversationId,
                        Content = userInput,
                        Type = MessageType.User
                    });

                    var streamContentTask = _client.ProcessContinuousConversationAsync(
                        [.. _messageHistory.Messages],
                        systemMessage: [ContentBlock.FromString(SystemPrompt)]);

                    var userInputContentBlock = ContentBlock.FromString();
                    var assistantMessage = new Message
                    {
                        Role = "assistant",
                        Content = [userInputContentBlock]
                    };
                    _messageHistory.AddMessage(assistantMessage);

                    var assistantContentBuilder = new StringBuilder();
                    await foreach (var streamContent in streamContentTask)
                    {
                        _userInterface.UpdateContentBlockText(userInputContentBlock, streamContent);
                        assistantContentBuilder.Append(streamContent);
                        await Task.Delay(50);
                    }

                    // Log assistant message
                    _conversationLogger.LogMessage(new MessageLogEntry
                    {
                        ConversationId = conversation.ConversationId,
                        Content = assistantContentBuilder.ToString(),
                        Type = MessageType.Agent,
                        AgentName = "Claude"
                    });
                }
            }
            finally
            {
                _conversationLogger.EndConversation(conversation.ConversationId);
            }
        }
    }
}
