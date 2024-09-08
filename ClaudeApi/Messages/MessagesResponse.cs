using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ClaudeApi.Messages
{
    public class MessagesResponse
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("role")]
        public string? Role { get; set; }

        [JsonProperty("content")]
        public List<ContentBlock>? Content { get; set; }

        [JsonProperty("model")]
        public string? Model { get; set; }

        [JsonProperty("stop_reason")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StopReason? StopReason { get; set; }

        [JsonProperty("stop_sequence")]
        public string? StopSequence { get; set; }

        [JsonProperty("usage")]
        public Usage? Usage { get; set; }

        public List<Message>? Messages { get; set; }
    }
}
