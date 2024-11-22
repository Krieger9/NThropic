using ClaudeApi.Messages;
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
}
