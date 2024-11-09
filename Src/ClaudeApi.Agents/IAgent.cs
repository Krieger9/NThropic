
namespace ClaudeApi.Agents
{
    public interface IAgent
    {
        public string SystemPrompt { get; }
        Task<string> ExecuteAsync(string input, Dictionary<string, object> arguments);
    }
}