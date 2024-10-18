namespace ClaudeApi.Agents
{
    public class Agent : IAgent
    {
        public virtual string SystemPrompt { get; set; } = "You are a helpful assistant.";
        public virtual async Task<string> ExecuteAsync(string input, Dictionary<string, object> arguments)
        {
            // Add your code here to process the input and arguments
            // Return the result
            return await Task.FromResult("");
        }
    }
}
