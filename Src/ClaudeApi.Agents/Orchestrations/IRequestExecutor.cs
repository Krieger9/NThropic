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
        CHALLENGE_LEVEL DefaultChallengeLevel { get; set; }

        IConverterAgent ConverterAgent { get; }
        IChallengeLevelAssesementAgent ChallengeLevelAssesementAgent { get; }
        ISmartClient Client { get; }
        IPromptService PromptService { get; }

        IRequestExecutor AddArguments(Dictionary<string, object> addArgs);
        IRequestExecutor Ask(string ask, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor Ask(List<string> asks, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ThenAsk(string ask, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ThenAsk(List<string> asks, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor Ask(Prompt prompt, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor Ask(List<Prompt> prompts, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ThenAsk(Prompt prompt, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ThenAsk(List<Prompt> prompts, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ConvertTo<T>();
        IRequestExecutor ProcessByAgent(IAgent agent);
        IRequestExecutor Contextualize();
        Task<IRequestExecutor> ExecuteAsync();
        Task<T?> AsAsync<T>();

        void Clear();
    }
}
