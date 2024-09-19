using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClaudeApi.Messages
{
    public class ContentBlockConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ContentBlock).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string? type = (string?)jo["type"];

            switch (type)
            {
                case "text_delta":
                case "text":
                    return new ContentBlock<TextContent>(type) { Content = jo.ToObject<TextContent>(serializer) };
                case "tool_result":
                    return new ToolResultContentBlock { Content = jo?.ToObject<ToolResultContentBlock>(serializer) };
                case "tool_use":
                    {
                        var id = jo["id"]?.ToString();
                        var name = jo["name"]?.ToString();
                        if (jo["input"] is JObject input)
                        {
                            if (id == null || name == null)
                            {
                                throw new JsonSerializationException("Invalid or missing properties for tool_use content block.");
                            }

                            return new ToolUseContentBlock
                            {
                                Id = id,
                                Name = name,
                                Input = input
                            };
                        }
                        throw new JsonSerializationException("Invalid or missing properties for tool_use content block.");
                    }
                case "input_json_delta":
                    return new ContentBlock<InputJsonContent>("input_json_delta") { Content = jo?.ToObject<InputJsonContent>(serializer) };
                case "image":
                    return new ContentBlock<ImageContent>("image") { Content = jo?.ToObject<ImageContent>(serializer) };
                case null:
                    return null;
                default:
                    throw new JsonSerializationException($"Unsupported content type: {type}");
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var contentBlock = (ContentBlock)value!;
            var jo = new JObject
            {
                ["type"] = contentBlock.Type
            };

            if(contentBlock.CacheControl != null)
            {
                jo["cache_control"] = contentBlock.CacheControl;
            }

            // Check if contentBlock.Content is not null
            if (contentBlock.GetType().IsGenericType && contentBlock.GetType().GetGenericTypeDefinition() == typeof(ContentBlock<>))
            {
                var contentProperty = contentBlock.GetType().GetProperty("Content");
                var contentValue = contentProperty?.GetValue(contentBlock);
                if (contentValue != null)
                {
                    jo["content"] = JToken.FromObject(contentValue, serializer);
                }
            }

            // Check if contentBlock is of type TextContent and add "text" property
            else if (contentBlock is TextContentBlock textContent)
            {
                jo["text"] = textContent.Text;
            }
            else if (contentBlock is ToolResultContentBlock toolResultContent)
            {
                jo["tool_use_id"] = toolResultContent.ToolUseId;
                var contentProperty = contentBlock.GetType().GetProperty("Content");
                var contentValue = contentProperty?.GetValue(contentBlock);
                if (contentValue != null)
                {
                    jo["content"] = JToken.FromObject(contentValue, serializer);
                }
            }
            else if (contentBlock is ToolUseContentBlock toolUseContent)
            {
                jo["id"] = toolUseContent.Id;
                jo["name"] = toolUseContent.Name;
                jo["input"] = toolUseContent.Input;
            }
            //            else if (contentBlock is InputJsonContent inputJsonContent)
            //            {
            //                jo["partial_json"] = inputJsonContent.PartialJson;
            //            }
            //            else if (contentBlock is ImageContent imageContent)
            //            {
            //                jo["source"] = JToken.FromObject(imageContent.Source, serializer);
            //            }
            jo.WriteTo(writer);
        }
    }
}
