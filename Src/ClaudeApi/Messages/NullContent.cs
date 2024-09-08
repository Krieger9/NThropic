using Newtonsoft.Json.Linq;

namespace ClaudeApi.Messages
{
    public class NullContent : ContentBlock<Object>
    {
        public NullContent() : base("null") { }
        public override JToken? GetContent()
        {
            return null;
        }
    }
}
