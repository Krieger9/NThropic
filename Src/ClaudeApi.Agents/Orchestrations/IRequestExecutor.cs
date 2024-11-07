using ClaudeApi.Agents.Agents;
using ClaudeApi.Agents.Agents.Converters;
using ClaudeApi.Agents.Contexts;
using ClaudeApi.Prompts;
using ClaudeApi.Services;

namespace ClaudeApi.Agents.Orchestrations
{
    public interface IRequestExecutor
    {
        string Contents { get; }

        IConverterAgent ConverterAgent { get; }
        IChallengeLevelAssesementAgent ChallengeLevelAssesementAgent { get; }
        ISmartClient Client { get; }
        IPromptService PromptService { get; }

        IRequestExecutor Ask(string ask);
        IRequestExecutor Ask(List<string> asks);
        IRequestExecutor ThenAsk(string ask);
        IRequestExecutor ThenAsk(List<string> asks);
        IRequestExecutor Ask(Prompt prompt);
        IRequestExecutor Ask(List<Prompt> prompts);
        IRequestExecutor ThenAsk(Prompt prompt);
        IRequestExecutor ThenAsk(List<Prompt> prompts);
        IRequestExecutor ConvertTo<T>();
        IRequestExecutor ProcessByAgent(IAgent agent);
        Task<IRequestExecutor> ExecuteAsync();
        Task<T?> AsAsync<T>();
    }
}
