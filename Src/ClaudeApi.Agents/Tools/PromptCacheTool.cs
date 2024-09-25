using ClaudeApi.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Tools
{
    public class PromptCacheTools(ISandboxFileManager sandboxFileManager)
    {
        [Tool("load_file_to_cache")]
        [Description("Caches the given information for later use. Useful for long contents and commonly used reference information.  Cached items will be references in the system prompt.  ###Cached data ###\\n.")]
        public static ToolInvokeResult LoadFileToPromptCache(string fileName)
        {
            return new ToolInvokeResult("Success", (client, history) =>
            {
                client.AddContextFile(fileName);
            });
        }

        [Tool("get_file_names_for_path")]
        [Description("Gets the file names under the specified path. Useful for listing files in a directory.")]
        public ToolInvokeResult GetFileNamesForPath(string relativePath, bool includeSubdirectories)
        {
            return new ToolInvokeResult("Success", (client, history) =>
            {
                var fileNames = sandboxFileManager.GetFileNamesForPath(relativePath, includeSubdirectories);

                // Assuming you want to add these file names to the context or return them in some way
                foreach (var fileName in fileNames)
                {
                    client.AddContextFile(fileName);
                }
            });
        }
    }
}
