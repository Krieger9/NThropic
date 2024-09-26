using ClaudeApi.Agents.Agents;
using ClaudeApi.Agents.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return services;
        }
    }
}
