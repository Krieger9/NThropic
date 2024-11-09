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

    public class ConvertTo<T> : IExecute, IObjectValue
    {
        public Object? ObjectValue { get; set; }

        public async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            //var result = await requestExtractor.ConverterAgent.ExecuteAsync(requestExtractor.Contents, new Dictionary<string, object> { { "desiredType", typeof(T) } });
            ObjectValue = await requestExtractor.ConverterAgent.ConvertToAsync(requestExtractor.Contents, typeof(T));
            return JsonConvert.SerializeObject(ObjectValue);
        }
    }

    public class Ask : IExecute
    {
        public CHALLENGE_LEVEL ChallengeLevel { get; set; }
        public string? Prompt { get; set; }

        public virtual async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            if (string.IsNullOrWhiteSpace(Prompt))
            {
                throw new Exception($"Prompt is not set {nameof(Ask)}");
            }
            return await requestExtractor.Client.ProcessContinuousConversationAsync(Prompt, ChallengeLevel).ToSingleStringAsync();
        }

        public override string ToString()
        {
            return Prompt ?? "";
        }
    }

    public class PromptAsk : IExecute
    {
        public Prompt? Prompt { get; set; }
        public string? ResolvedPrompt { get; set; }
        public CHALLENGE_LEVEL ChallengeLevel { get; set; }

        public async Task<string> ExecuteAsync(IRequestExecutor requestExecutor)
        {
            if (Prompt == null)
            {
                throw new Exception($"AdditionalPrompt is not set in {nameof(PromptAsk)}");
            }

            var (result, resolvedPrompt) = await requestExecutor.Client.ProcessContinuousConversationAsync(Prompt, [], ChallengeLevel, null);
            ResolvedPrompt = resolvedPrompt;
            return result;
        }

        public override string ToString()
        {
            return $"{ResolvedPrompt}";
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
            return await Agent.ExecuteAsync(requestExtractor.Contents, []);
        }
    }
}
