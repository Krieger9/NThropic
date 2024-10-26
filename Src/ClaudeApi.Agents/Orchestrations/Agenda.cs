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

    public class ConvertTo<T> : IExecute
    {
        public async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            var result = await requestExtractor.ConverterAgent.ExecuteAsync(requestExtractor.Contents, []);
            return JsonConvert.SerializeObject(result);
        }
    }

    public class Ask: IExecute
    {
        public CHALLENGE_LEVEL ChallengeLevel { get; set; }
        public string? Prompt { get; set; }
        public virtual async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            if(string.IsNullOrWhiteSpace(Prompt))
            {
                throw new Exception($"Prompt is not set {nameof(Ask)}");
            }
            return await requestExtractor.Client.ProcessContinuousConversationAsync(Prompt, ChallengeLevel).ToSingleStringAsync();
        }
    }

    public class PromptAsk : Ask
    {
        public Prompt? AdditionalPrompt { get; set; }

        public override async Task<string> ExecuteAsync(IRequestExecutor requestExecutor)
        {
            if (AdditionalPrompt == null)
            {
                throw new Exception($"AdditionalPrompt is not set in {nameof(PromptAsk)}");
            }

            if (string.IsNullOrWhiteSpace(Prompt))
            {
                var message = await requestExecutor.PromptService.ParsePromptAsync(AdditionalPrompt);
                Prompt = message.Content?.FirstOrDefault()?.ToString();
            }

            return await base.ExecuteAsync(requestExecutor);
        }
    }

    public class AgentExecutable : IExecute
    {
        public IAgent? Agent { get; set; }
        public async Task<string> ExecuteAsync(IRequestExecutor requestExtractor)
        {
            if(Agent == null)
            {
                throw new Exception($"Agent is not set {nameof(AgentExecutable)}");
            }
            return await Agent.ExecuteAsync(requestExtractor.Contents, []);
        }
    }
}
