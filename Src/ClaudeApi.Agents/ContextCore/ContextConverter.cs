using ClaudeApi.Agents.ContextCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace ClaudeApi.Agents.ContextCore
{
    public class ContextConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IContext).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var contextType = jsonObject["Type"]?.Value<string>() ?? "Context";

            IContext context;
            switch (contextType)
            {
                case "Context":
                    var summary = jsonObject["Summary"]?.Value<string>() ?? string.Empty;
                    var details = jsonObject["Details"]?.Value<string>() ?? string.Empty;
                    context = new Context(summary, details);
                    break;
                // Add cases for other IContext implementations
                default:
                    throw new NotSupportedException($"Unknown context type: {contextType}");
            }

            serializer.Populate(jsonObject.CreateReader(), context);
            return context;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var context = (IContext)value;
            var jsonObject = JObject.FromObject(context, serializer);
            jsonObject.AddFirst(new JProperty("Type", context.GetType().Name));
            jsonObject.WriteTo(writer);
        }
    }
}
