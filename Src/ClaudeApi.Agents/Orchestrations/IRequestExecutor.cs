using ClaudeApi.Agents.Agents;
using ClaudeApi.Agents.Agents.Converters;
using ClaudeApi.Prompts;
using ClaudeApi.Services;

namespace ClaudeApi.Agents.Orchestrations
{
    public interface IRequestExecutor
    {
        IConverterAgent ConverterAgent { get; }
        IChallengeLevelAssesementAgent ChallengeLevelAssesementAgent { get; }
        ISmartClient Client { get; }
        IPromptService PromptService { get; }
        string Contents { get; }
        IRequestExecutor Ask(string ask);
        IRequestExecutor Ask(List<string> asks);
        IRequestExecutor ThenAsk(string ask);
        IRequestExecutor ThenAsk(List<string> asks);
        IRequestExecutor Ask(Prompt prompt); // New method
        IRequestExecutor Ask(List<Prompt> prompts); // New method
        IRequestExecutor ThenAsk(Prompt prompt); // New method
        IRequestExecutor ThenAsk(List<Prompt> prompts); // New method
        IRequestExecutor ConvertTo<T>();
        IRequestExecutor ProcessByAgent(IAgent agent);
        Task<string> Execute();
    }
}
