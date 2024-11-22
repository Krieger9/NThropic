namespace ClaudeApi.Agents.Orchestrations
{
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
}
