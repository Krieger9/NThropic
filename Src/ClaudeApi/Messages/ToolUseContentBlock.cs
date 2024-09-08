using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClaudeApi.Messages
{
    public class ToolUseContentBlock : ContentBlock<object>
    {
        public ToolUseContentBlock() : base("tool_use") { }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("input")]
        public JObject? Input { get; set; }

        public override JToken? GetContent()
        {
            return JToken.FromObject(this);
        }
    }
}
