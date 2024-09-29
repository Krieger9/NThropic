using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClaudeApi.Messages
{

    [JsonConverter(typeof(MessageConverter))]
    public class Message
    {
        [JsonProperty("role")]
        public string? Role { get; set; }

        [JsonProperty("content")]
        public List<ContentBlock>? Content { get; set; } // Change to object to support both string and List<ContentBlock>

        public override string ToString()
        {
            if(Content == null) return string.Empty;
            else
            return $"{Role}: {string.Join("\n", Content)}";
        }
    }

    public class MessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Message);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var message = new Message
            {
                Role = (string?)jo["role"]
            };

            JToken? contentToken = jo["content"];
            if (contentToken?.Type == JTokenType.String)
            {
                message.Content = [ContentBlock.FromString(contentToken.ToString())];
            }

            else if (contentToken?.Type == JTokenType.Array)
            {
                message.Content = contentToken.ToObject<List<ContentBlock>>(serializer);
            }
            else
            {
                throw new JsonSerializationException("Unexpected content type");
            }

            return message;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var message = (Message?)value;
            var jo = new JObject
            {
                ["role"] = message?.Role
            };

            if (message?.Content is not null)
            {
                jo["content"] = JToken.FromObject(message.Content, serializer);
            }
            else
            {
                throw new JsonSerializationException("Unexpected content type");
            }

            jo.WriteTo(writer);
        }
    }
}
