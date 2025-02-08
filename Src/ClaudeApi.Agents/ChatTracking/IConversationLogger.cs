namespace ClaudeApi.Agents.ChatTracking
{
    // Logger interface
    public interface IConversationLogger
    {
        void SaveConversation(Conversation conversation);
        void LogMessage(MessageLogEntry entry);
        Conversation? GetConversation(Guid conversationId);
        void EndConversation(Guid conversationId);
    }
}
