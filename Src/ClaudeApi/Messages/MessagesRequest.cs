using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ClaudeApi.Messages
{
    public class MessagesRequest
    {
        [JsonProperty("model")]
        public string? Model { get; set; }

        [JsonProperty("system")]
        public List<ContentBlock>? SystemMessage { get; set; }

        [JsonProperty("messages")]
        public List<Message>? Messages { get; set; }

        [JsonProperty("max_tokens")]
        public int? MaxTokens { get; set; }

        [JsonProperty("temperature")]
        public double? Temperature { get; set; }

        [JsonProperty("stream")]
        public bool? Stream { get; set; }

        [JsonProperty("tools")]
        public List<ToolInfo>? Tools { get; set; }

        public class ToolInfo
        {
            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("description")]
            public string? Description { get; set; }

            [JsonProperty("input_schema")]
            public object? InputSchema { get; set; }

            [JsonProperty("cache_control", NullValueHandling = NullValueHandling.Ignore)]
            public JObject? CacheControl { get; set; }
        }

        public class FileContent
        {
            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("content")]
            public string? Content { get; set; }
        }
    }
}
