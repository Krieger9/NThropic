using ClaudeApi.Messages;
using ClaudeApi.Tools;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace ClaudeApi.Services
{
    public interface IToolManagementService
    {
        IToolRegistry ToolRegistry { get; }

        void DiscoverTools(Assembly assembly);
        void DiscoverTools(Type type);
        void DiscoverTool(Type type, string methodName);

        Task<string> ExecuteToolAsync(string toolName, JObject input, Client client, List<Message> messages);
    }
}