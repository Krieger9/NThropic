using Newtonsoft.Json;

namespace ClaudeApi.Messages
{
    public class InputJsonContent
    {
        [JsonProperty("partial_json")]
        public string? PartialJson { get; set; }
    }
}
