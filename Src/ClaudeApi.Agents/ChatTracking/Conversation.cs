using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClaudeApi.Agents.ChatTracking
{
    public class Conversation
    {
        public Guid ConversationId { get; set; } = Guid.NewGuid();
        public Guid? ParentConversationId { get; set; }
        public List<MessageLogEntry> Messages { get; set; } = [];
        public Dictionary<string, AgentInfo> Agents { get; set; } = [];
    }
}
