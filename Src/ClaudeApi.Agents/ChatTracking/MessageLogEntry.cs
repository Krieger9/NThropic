namespace ClaudeApi.Agents.ChatTracking
{
    public class MessageLogEntry
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public string? AgentName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Content { get; set; }
        public MessageType Type { get; set; }
        public string? MetaData { get; set; }
        public Guid? SubConversationId { get; set; }
    }
}
