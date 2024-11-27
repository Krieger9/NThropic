using ClaudeApi.Prompts;
using Newtonsoft.Json;

[JsonConverter(typeof(CacheKeyConverter))]
public class PromptCacheKey : ICacheKey
{
    public Prompt Prompt { get; }

    public PromptCacheKey(Prompt prompt)
    {
        Prompt = prompt;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PromptCacheKey other) return false;
        return Prompt.Equals(other.Prompt);
    }

    public override int GetHashCode()
    {
        return Prompt.GetHashCode();
    }
}
