namespace ClaudeApi.Agents.Orchestrations
{
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
}
