
namespace ClaudeApi.Agents
{
    public interface IAgent
    {
        Task<string> ExecuteAsync(string input, Dictionary<string, object> arguments);
    }
}