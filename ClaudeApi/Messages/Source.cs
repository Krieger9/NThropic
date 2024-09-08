using Newtonsoft.Json;

namespace ClaudeApi.Messages
{
    public class Source
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("media_type")]
        public string? MediaType { get; set; }

        [JsonProperty("data")]
        public string? Data { get; set; }
    }
}
