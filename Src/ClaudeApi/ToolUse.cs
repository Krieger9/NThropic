using System.Text;
using Newtonsoft.Json.Linq;

namespace ClaudeApi
{
    public class ToolUse(string id, string toolName)
    {
        public string Id { get; } = id;
        public string ToolName { get; } = toolName;
        public JObject Input { get; private set; } = [];

        private readonly StringBuilder _inputAccumulator = new ();

        public void AccumulateInput(string partialJson)
        {
            _inputAccumulator.Append(partialJson);
        }

        public JObject CompleteInputAccumulation()
        {
            Input = JObject.Parse(_inputAccumulator.ToString());
            return Input;
        }
    }
}