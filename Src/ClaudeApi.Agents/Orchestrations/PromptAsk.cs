using ClaudeApi.Prompts;

namespace ClaudeApi.Agents.Orchestrations
{
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
}
