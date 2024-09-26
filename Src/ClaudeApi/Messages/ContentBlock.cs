using ClaudeApi.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Messages
{
    [JsonConverter(typeof(ContentBlockConverter))]
    public abstract class ContentBlock
    {
        [JsonProperty("type")]
        public abstract string? Type { get; }

        [JsonProperty("cache_control", NullValueHandling = NullValueHandling.Ignore)]
        public JObject? CacheControl { get; set; }

        public abstract JToken? GetContent();

        // Helper method to create a ContentBlock from a string
        public static ContentBlock FromString(string content, JObject? cacheControl = null)
        {
            return new TextContentBlock()
            {
                Text = content,
                CacheControl = cacheControl
            };
        }

        override public string ToString()
        {
            return GetContent()?.ToString() ?? string.Empty;
        }
    }

    public class ContentBlock<T> : ContentBlock
    {
        [JsonProperty("type")]
        public override string? Type { get; }

        [JsonProperty("content")]
        public T? Content { get; set; }

        public ContentBlock(string? type = null)
        {
            if (type != null)
            {
                Type = type;
            }
            else
            {
                Type = typeof(T) switch
                {
                    Type T when T == typeof(TextContentBlock) => "text",
                    Type T when T == typeof(ToolResultContentBlock) => "tool_result",
                    Type T when T == typeof(ToolUseContentBlock) => "tool_use",
                    Type T when T == typeof(InputJsonContent) => "input_json_delta",
                    Type T when T == typeof(ImageContent) => "image",
                    _ => "object"
                };
            }
        }

        public override JToken? GetContent()
        {
            return Content != null ? JToken.FromObject(Content) : null;
        }
    }
}
