using ClaudeApi.Agents.Orchestrations;

namespace ClaudeApi.Agents.Agents.Configs
{
    public class ContextualizeAgentConfig
    {
        public CHALLENGE_LEVEL ChallengeLevel { get; set; } = CHALLENGE_LEVEL.INTERMEDIATE;
        public int SummaryLengthThreshold { get; set; } = 1024;
    }
}
