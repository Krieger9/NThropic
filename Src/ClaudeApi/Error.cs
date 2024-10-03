using Newtonsoft.Json;

namespace ClaudeApi
{
    public class Error
    {
        [JsonProperty("message")]
        public string? Message { get; set; }
    }
}