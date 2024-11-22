using ClaudeApi.Agents.Orchestrations;

namespace ClaudeApi.Agents.Agents.Configs
{
    public class ContextualizeAgentConfig
    {
        public string? Model { get; set; }
        public int SummaryLengthThreshold { get; set; } = 1024;
        public string? BaseContextualizePromptFile { get; set; } = "ContextualizeAsks";
        public string? ContextAsBackdropPromptFile { get; set; }
    }
}
