using System.Reflection;
using ClaudeApi.Messages;
using ClaudeApi.Tools;
using Microsoft.Extensions.Logging;

namespace ClaudeApi.Services
{
    public class ToolManager
    {
        private readonly ToolDiscoveryService _toolDiscoveryService;
        private readonly ToolExecutionService _toolExecutionService;
        private readonly IToolRegistry _toolRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ToolManager> _logger;

        public ToolManager(
            IServiceProvider serviceProvider,
            IToolRegistry toolRegistry,
            ILogger<ToolManager> logger)
        {
            _serviceProvider = serviceProvider;
            _toolDiscoveryService = new ToolDiscoveryService(_serviceProvider);
            _toolRegistry = toolRegistry;
            _toolExecutionService = new ToolExecutionService(_toolRegistry);
            _logger = logger;
        }

        public void DiscoverTools(Assembly assembly)
        {
            var discoveredTools = _toolDiscoveryService.DiscoverTools(assembly);
            _toolRegistry.AddTools(discoveredTools);
            _logger.LogInformation("Discovered {ToolCount} tools", discoveredTools.Count);
        }

        public void DiscoverTools(Type type)
        {
            var tools = _toolDiscoveryService.DiscoverTools(type);
            _toolRegistry.AddTools(tools);
            _logger.LogInformation("Discovered {ToolCount} tools from type {TypeName}", tools.Count, type.Name);
        }

        public void DiscoverTool(Type type, string methodName)
        {
            var tool = _toolDiscoveryService.DiscoverTool(type, methodName);
            if (tool != null)
            {
                _toolRegistry.AddTool(tool);
                _logger.LogInformation("Discovered tool {ToolName} from type {TypeName}", tool.Name, type.Name);
            }
            else
            {
                _logger.LogWarning("No tool found with method name {MethodName} in type {TypeName}", methodName, type.Name);
            }
        }

        public async Task<ToolResult> ExecuteToolAsync(ToolUse toolUse, List<Message> messages, Client client)
        {
            try
            {
                var result = await _toolExecutionService.ExecuteToolAsync(toolUse.ToolName, toolUse.Input, client, messages);
                return new ToolResult(toolUse.Id, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool {ToolName}", toolUse.ToolName);
                return new ToolResult(toolUse.Id, $"Error: {ex.Message}", isError: true);
            }
        }

        public List<MessagesRequest.ToolInfo> GetRegisteredTools()
        {
            return _toolRegistry.Tools.Select(t => new MessagesRequest.ToolInfo
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.InputSchema ?? throw new InvalidOperationException($"{nameof(t.InputSchema)} cannot be null.")
            }).ToList();
        }
    }
}
