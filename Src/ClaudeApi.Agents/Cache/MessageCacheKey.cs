using ClaudeApi.Agents.Orchestrations;
using ClaudeApi.Messages;
using Newtonsoft.Json;

[JsonConverter(typeof(CacheKeyConverter))]
public class MessageCacheKey : ICacheKey
{
    public List<Message> Messages { get; }
    public List<ContentBlock>? ContentBlocks { get; }
    public CHALLENGE_LEVEL ChallengeLevel { get; }

    public MessageCacheKey(List<Message> messages, List<ContentBlock>? systemMessages, CHALLENGE_LEVEL challengeLevel)
    {
        Messages = messages;
        ContentBlocks = systemMessages;
        ChallengeLevel = challengeLevel;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MessageCacheKey other) return false;
        return Messages.SequenceEqual(other.Messages) &&
               (ContentBlocks == null && other.ContentBlocks == null || ContentBlocks != null && other.ContentBlocks != null && ContentBlocks.SequenceEqual(other.ContentBlocks)) &&
               ChallengeLevel == other.ChallengeLevel;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            // Start with a non-zero value to ensure 0 inputs are handled well
            int hash = 17;

            // XOR is perfect here since order doesn't matter!
            foreach (var message in Messages)
            {
                hash ^= message.GetHashCode();
            }

            if (ContentBlocks != null)
            {
                foreach (var block in ContentBlocks)
                {
                    hash ^= block.GetHashCode();
                }
            }

            hash ^= ChallengeLevel.GetHashCode();

            return hash;
        }
    }
}