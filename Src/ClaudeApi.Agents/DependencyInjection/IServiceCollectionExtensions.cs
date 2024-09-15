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
        public static void AddNThropicAgents(this IServiceCollection services, Client client)
        {
            services.AddSingleton(new SummaryAgent(client));
            services.AddTransient<TestTools>();
            services.AddTransient<SummarizeTools>();
            services.AddTransient<PromptCacheTools>();
            services.AddTransient<DiskTools>();
        }
    }
}
