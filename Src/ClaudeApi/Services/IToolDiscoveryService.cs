using ClaudeApi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Services
{
    public interface IToolDiscoveryService
    {
        List<Tool> DiscoverTools(Assembly assembly);
        List<Tool> DiscoverTools(Type type);
        Tool? DiscoverTool(Type type, string methodName);
    }
}
