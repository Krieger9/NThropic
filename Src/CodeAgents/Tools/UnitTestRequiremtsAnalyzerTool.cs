using ClaudeApi.Tools;
using CodeAgents.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CodeAgents.Tools
{
    public class UnitTestRequirementsAnalyzerTool(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        [Tool("analyze_unit_test_requirements")]
        [Description("Analyzes the unit test requirements for a given class and file")]
        public async Task<string> AnalyzeUnitTestRequirementsAsync(string className, string fileName)
        {
            var analyzerAgent = _serviceProvider.GetRequiredService<UnitTestRequirementsAnalyzerAgent>();

            var arguments = new Dictionary<string, object>
            {
                { "file_name", fileName }
            };

            return await analyzerAgent.ExecuteAsync(className, arguments);
        }
    }
}
