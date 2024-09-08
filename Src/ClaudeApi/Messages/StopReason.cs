using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ClaudeApi.Messages
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StopReason
    {
        [EnumMember(Value = "end_turn")]
        EndTurn,

        [EnumMember(Value = "max_tokens")]
        MaxTokens,

        [EnumMember(Value = "stop_sequence")]
        StopSequence,

        [EnumMember(Value = "tool_use")]
        ToolUse
    }
}
