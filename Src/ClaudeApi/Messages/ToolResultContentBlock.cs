using Newtonsoft.Json;

namespace ClaudeApi.Messages
{
    public class ToolResultContentBlock : ContentBlock<object>
    {
        public ToolResultContentBlock() : base("tool_result") { }

        [JsonProperty("tool_use_id")]
        public string? ToolUseId { get; set; }
    }
}
