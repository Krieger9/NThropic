
using ClaudeApi.Agents.Orchestrations;
using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CacheKeyConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(ICacheKey).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["Type"]?.ToString();
        var keyJson = jsonObject["Key"]?.ToString();

        if (keyJson == null || type == null)
        {
            return null;
        }

        ICacheKey? key = null;
        if (type == nameof(MessageCacheKey))
        {
            var (messages, systemMessages, challengeLevel) =
                JsonConvert.DeserializeObject<Tuple<List<Message>, List<ContentBlock>, CHALLENGE_LEVEL>>(keyJson)
                ?? throw new JsonException();
            key = new MessageCacheKey(messages, systemMessages, challengeLevel);
            return JsonConvert.DeserializeObject<MessageCacheKey>(keyJson);
        }
        else if (type == nameof(PromptCacheKey))
        {
            key = new PromptCacheKey(
                JsonConvert.DeserializeObject<Prompt>(keyJson) ?? throw new JsonException("Prompt cannot be null.")
            );
        }
        return key ??
            throw new NotSupportedException($"Type '{type}' is not supported for deserialization.");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        if (!(value is ICacheKey cacheKey))
        {
            throw new NotSupportedException($"Type '{value?.GetType().Name ?? "<null>"}' is not supported for serialization.");
        }

        writer.WriteStartObject();
        writer.WritePropertyName("Type");
        writer.WriteValue(value.GetType().Name);

        writer.WritePropertyName("Key");
        if (cacheKey is MessageCacheKey messageCacheKey)
        {
            serializer.Serialize(writer, messageCacheKey.Messages);
        }
        else if (cacheKey is PromptCacheKey promptCacheKey)
        {
            serializer.Serialize(writer, promptCacheKey.Prompt);
        }
        else
        {
            throw new NotSupportedException($"Type '{value.GetType().Name}' is not supported for serialization.");
        }

        writer.WriteEndObject();
    }
}
