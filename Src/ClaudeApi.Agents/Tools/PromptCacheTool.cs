using ClaudeApi.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Tools
{
    public class PromptCacheTools
    {
        [Tool("load_file_to_cache")]
        [Description("Caches the given information for later use. Useful for long contents and commonly used reference information.")]
        public static ToolInvokeResult LoadFileToPromptCache(string fileName)
        {
            return new ToolInvokeResult("Success", (client, history) =>
            {
                client.AddContextFile(fileName);
            });
        }
    }
}
