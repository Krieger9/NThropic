using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClaudeApi.Messages
{
    public class TextContentBlock : ContentBlock<string>
    {
        public TextContentBlock() : base("text") { }

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        public override JToken? GetContent()
        {
            return JToken.Parse(Text);
        }
    }
}
