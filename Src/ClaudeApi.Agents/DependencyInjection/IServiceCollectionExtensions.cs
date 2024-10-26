using ClaudeApi.Agents.Agents;
using ClaudeApi.Agents.Agents.Converters;
using ClaudeApi.Agents.Orchestrations;
using ClaudeApi.Agents.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeApi.Agents.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddNThropicAgents(this IServiceCollection services)
        {
            services.AddSingleton<SummaryAgent>();
            services.AddTransient<TestTools>();
            services.AddTransient<SummarizeTools>();
            services.AddTransient<PromptCacheTools>();
            services.AddTransient<DiskTools>();
            services.AddTransient<IConverterAgent, GenericConverterAgent>();
            services.AddTransient<IChallengeLevelAssesementAgent, ChallengeLevelAssesementAgent>();
            services.AddTransient<ISmartClient, SmartClient>();
            return services;
        }
    }
}
