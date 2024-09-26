using Newtonsoft.Json;

namespace ClaudeApi.Messages
{
    public class TextContent
    {
        [JsonProperty("text")]
        public string? Text { get; set; }

        public override string ToString()
        {
            return Text ?? string.Empty;
        }
    }
}
