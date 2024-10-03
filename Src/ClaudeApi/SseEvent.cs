using Newtonsoft.Json;
using ClaudeApi.Messages;

namespace ClaudeApi
{
    public class SseEvent
    {
        [JsonProperty("message")]
        public MessagesResponse? Message { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("content_block")]
        public ContentBlock? ContentBlock { get; set; }

        [JsonProperty("delta")]
        public ContentBlock? Delta { get; set; }

        [JsonProperty("error")]
        public Error? Error { get; set; }
    }
}