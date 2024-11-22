using ClaudeApi.Agents.Agents;
using ClaudeApi.Agents.Agents.Converters;
using ClaudeApi.Agents.ContextCore;
using ClaudeApi.Prompts;
using ClaudeApi.Services;

namespace ClaudeApi.Agents.Orchestrations
{
    public interface IRequestExecutor
    {
        CHALLENGE_LEVEL DefaultChallengeLevel { get; set; }

        IConverterAgent ConverterAgent { get; }
        IChallengeLevelAssesementAgent ChallengeLevelAssesementAgent { get; }
        IContextualizeAgent ContextualizeAgent { get; }
        ISmartClient Client { get; }
        IContext? Context { get; set; }
        IPromptService PromptService { get; }
        IDictionary<string, object> BaseArguments { get; }

        IRequestExecutor AddArguments(Dictionary<string, object> addArgs);
        IRequestExecutor Ask(string ask, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor Ask(List<string> asks, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ThenAsk(string ask, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ThenAsk(List<string> asks, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor Ask(Prompt prompt, Dictionary<string,object>? arguments = null, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor Ask(List<Prompt> prompts, Dictionary<string,object>? arguments = null, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ThenAsk(Prompt prompt, Dictionary<string,object>? arguments = null, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ThenAsk(List<Prompt> prompts, Dictionary<string, object>? arguments, CHALLENGE_LEVEL? challengeLevel = null);
        IRequestExecutor ProcessByAgent(IAgent agent);
        IRequestExecutor Contextualize();
        IRequestExecutor SetChallengeLevel(CHALLENGE_LEVEL challengeLevel);
        List<string> Information { get; }
        string InformationString { get; }

        Task<string> Result();
        Task<T> ConvertTo<T>();

        void Clear();
    }
}
