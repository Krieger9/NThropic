using ClaudeApi.Messages;
using ClaudeApi.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Services
{
    public interface IToolExecutionService
    {
        IToolRegistry ToolRegistry { get; }

        Task<string> ExecuteToolAsync(string toolName, JObject input, ClaudeClient client, List<Message> messages);
    }
}
