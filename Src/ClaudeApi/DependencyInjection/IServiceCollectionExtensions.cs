using ClaudeApi.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddClaudApi(this IServiceCollection services)
        {
            return services.AddSingleton<IClaudeApiService, ClaudeApiService>()
            .AddTransient<IToolManagementService, ToolManagementService>()
            .AddTransient<IToolExecutionService, ToolExecutionService>()
            .AddTransient<IToolDiscoveryService, ToolDiscoveryService>();
        }
    }
}
