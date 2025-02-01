using System.Diagnostics;

namespace ClaudeApi.Agents.ChatTracking.OpenTelemetry
{
    public class OpenTelemetryConversationLogger : IConversationLogger
    {
        // Create an ActivitySource for all our tracing
        private static readonly ActivitySource ActivitySource = new("AgentLogging");

        // In-memory stores for conversation data and their associated activities
        private readonly Dictionary<Guid, Conversation> _conversations = [];
        private readonly Dictionary<Guid, Activity> _conversationActivities = [];

        public void SaveConversation(Conversation conversation)
        {
            // Determine parent context if any.
            ActivityContext parentContext = default;
            if (conversation.ParentConversationId.HasValue &&
                _conversationActivities.TryGetValue(conversation.ParentConversationId.Value, out var parentActivity))
            {
                parentContext = parentActivity.Context;
            }

            // Start a new conversation activity with the parent context (if available)
            var conversationActivity = ActivitySource.StartActivity(
                "Conversation",
                ActivityKind.Internal,
                parentContext);

            conversationActivity?.SetTag("conversation.id", conversation.ConversationId.ToString());
            if (conversation.ParentConversationId.HasValue)
            {
                conversationActivity?.SetTag("conversation.parent.id", conversation.ParentConversationId.Value.ToString());
            }

            // Store the conversation and its activity
            _conversations[conversation.ConversationId] = conversation;
            if (conversationActivity != null)
            {
                _conversationActivities[conversation.ConversationId] = conversationActivity;
            }
        }

        public void LogMessage(MessageLogEntry entry)
        {
            if (!_conversations.TryGetValue(entry.ConversationId, out Conversation? conversation))
            {
                throw new InvalidOperationException("Conversation not found. Save the conversation before logging messages.");
            }

            conversation.Messages.Add(entry);

            // Retrieve the conversation activity to use as parent
            if (_conversationActivities.TryGetValue(entry.ConversationId, out var conversationActivity))
            {
                // Create a child activity for this message
                var messageActivity = ActivitySource.StartActivity(
                    "Message",
                    ActivityKind.Internal,
                    conversationActivity.Context);

                if (messageActivity != null)
                {
                    messageActivity.SetTag("message.id", entry.MessageId.ToString());
                    messageActivity.SetTag("agent.name", entry.AgentName);
                    messageActivity.SetTag("message.type", entry.Type.ToString());
                    messageActivity.SetTag("message.content", entry.Content);
                    messageActivity.SetTag("message.timestamp", entry.Timestamp.ToString("o"));
                    if (entry.SubConversationId.HasValue)
                    {
                        messageActivity.SetTag("message.subconversation.id", entry.SubConversationId.Value.ToString());
                    }
                    messageActivity.Stop();
                }
            }
        }

        public Conversation? GetConversation(Guid conversationId)
        {
            _conversations.TryGetValue(conversationId, out var conversation);
            return conversation;
        }

        public void EndConversation(Guid conversationId)
        {
            if (_conversationActivities.TryGetValue(conversationId, out var activity))
            {
                activity.Stop();
                _conversationActivities.Remove(conversationId);
            }
        }
    }
}
