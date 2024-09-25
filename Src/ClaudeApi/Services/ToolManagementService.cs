using System.Reflection;
using ClaudeApi.Messages;
using ClaudeApi.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ClaudeApi.Services
{
    public class ToolManagementService : IToolManagementService
    {
        private readonly IToolDiscoveryService _toolDiscoveryService;
        private readonly IToolExecutionService _toolExecutionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ToolManagementService> _logger;

        public IToolRegistry ToolRegistry => _toolExecutionService.ToolRegistry;

        public ToolManagementService(
            IServiceProvider serviceProvider,
            IToolRegistry toolRegistry,
            IToolDiscoveryService toolDiscoveryService,
            IToolExecutionService toolExecutionService,
            ILogger<ToolManagementService> logger)
        {
            _serviceProvider = serviceProvider;
            _toolDiscoveryService = toolDiscoveryService;
            _toolExecutionService = toolExecutionService;
            _logger = logger;
        }

        public void DiscoverTools(Assembly assembly)
        {
            var discoveredTools = _toolDiscoveryService.DiscoverTools(assembly);
            ToolRegistry.AddTools(discoveredTools);
            _logger.LogInformation("Discovered {ToolCount} tools", discoveredTools.Count);
        }

        public void DiscoverTools(Type type)
        {
            var tools = _toolDiscoveryService.DiscoverTools(type);
            ToolRegistry.AddTools(tools);
            _logger.LogInformation("Discovered {ToolCount} tools from type {TypeName}", tools.Count, type.Name);
        }

        public void DiscoverTool(Type type, string methodName)
        {
            var tool = _toolDiscoveryService.DiscoverTool(type, methodName);
            if (tool != null)
            {
                ToolRegistry.AddTool(tool);
                _logger.LogInformation("Discovered tool {ToolName} from type {TypeName}", tool.Name, type.Name);
            }
            else
            {
                _logger.LogWarning("No tool found with method name {MethodName} in type {TypeName}", methodName, type.Name);
            }
        }

        public async Task<string> ExecuteToolAsync(string toolName, JObject input, Client client, List<Message> messages)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                throw new ArgumentException("Tool name cannot be null or empty.");
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "Tool input cannot be null.");
            }

            if (!ToolRegistry.TryGetTool(toolName, out var tool))
            {
                throw new ArgumentException($"Tool '{toolName}' not found.");
            }

            // Validate that the tool is valid.
            // Null check is not necessary because the TryGetTool method will return null if the tool is not found.
            if(!tool!.IsValid())
            {
                throw new InvalidOperationException($"Tool '{toolName}' is not valid.");
            }
            // tool.IsValid will verify null checks for Name, Description, InputSchema, and Method
            var result = await _toolExecutionService.ExecuteToolAsync(tool.Name!, input, client, messages);

            return result;
        }

        public List<MessagesRequest.ToolInfo> GetRegisteredTools()
        {
            return ToolRegistry.Tools.Select(t => new MessagesRequest.ToolInfo
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.InputSchema ?? throw new InvalidOperationException($"{nameof(t.InputSchema)} cannot be null.")
            }).ToList();
        }
    }
}
