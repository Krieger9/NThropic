using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ClaudeApi.Agents.Orchestrations
{
    public enum CHALLENGE_LEVEL
    {
        AUTO = -1,
        NONE = 0,
        ELEMENTARY,
        INTERMEDIATE,
        PROFESSIONAL,
        EXPERT,
        LEADING_EXPERT
    }

    public interface IExecute
    {
        Task<string> ExecuteAsync(IRequestExecutor requestExtractor);
    }

    public interface IObjectValue
    {
        Object? ObjectValue { get; }
    }

    public class SetChallengeLevelExecutable : IExecute
    {
        private readonly CHALLENGE_LEVEL _challengeLevel;

        public SetChallengeLevelExecutable(CHALLENGE_LEVEL challengeLevel)
        {
            _challengeLevel = challengeLevel;
        }

        public async Task<string> ExecuteAsync(IRequestExecutor requestExecutor)
        {
            requestExecutor.SetChallengeLevel(_challengeLevel);
            return await Task.FromResult(string.Empty);
        }
    }

    public class Ask : IExecute
    {
        public CHALLENGE_LEVEL? ChallengeLevel { get; set; }
        public string? Prompt { get; set; }

        public virtual async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            if (string.IsNullOrWhiteSpace(Prompt))
            {
                throw new Exception($"Prompt is not set {nameof(Ask)}");
            }
            return await requestExtractor.Client.ProcessContinuousConversationAsync(
                Prompt, 
                ChallengeLevel ?? requestExtractor.DefaultChallengeLevel
            ).ToSingleStringAsync();
        }

        public override string ToString()
        {
            return Prompt ?? "";
        }
    }

    public class PromptAsk : IExecute
    {
        public Prompt? Prompt { get; set; }
        public IDictionary<string, object>? RunArguments { get; set; }
        public string? ResolvedPrompt { get; set; }
        public CHALLENGE_LEVEL? ChallengeLevel { get; set; }

        public async Task<string> ExecuteAsync(IRequestExecutor requestExecutor)
        {
            if (Prompt == null)
            {
                throw new Exception($"AdditionalPrompt is not set in {nameof(PromptAsk)}");
            }

            try
            {
                var arguments = CombineArguments(Prompt.Arguments, requestExecutor.BaseArguments, RunArguments);
                var cloned = Prompt.Clone(arguments, false);
                var (result, resolvedPrompt) = await requestExecutor.Client.ProcessContinuousConversationAsync(
                    cloned,
                    [],
                    ChallengeLevel ?? requestExecutor.DefaultChallengeLevel, null
                );
                ResolvedPrompt = resolvedPrompt;
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing prompt '{Prompt.Name}': {ex.Message}", ex);
            }
        }

        // Can we remove?
        public override string ToString()
        {
            return $"{ResolvedPrompt}";
        }

        private static Dictionary<string, object> CombineArguments(params IDictionary<string, object>?[] dictionaries)
        {
            var combinedArguments = new Dictionary<string, object>();

            foreach (var dict in dictionaries)
            {
                if (dict != null)
                {
                    foreach (var arg in dict)
                    {
                        combinedArguments[arg.Key] = arg.Value;
                    }
                }
            }

            return combinedArguments;
        }

    }

    public class AgentExecutable : IExecute
    {
        public IAgent? Agent { get; set; }
        public async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            if (Agent == null)
            {
                throw new Exception($"Agent is not set {nameof(AgentExecutable)}");
            }
            return await Agent.ExecuteAsync(requestExtractor.InformationString, []);
        }
    }
}
