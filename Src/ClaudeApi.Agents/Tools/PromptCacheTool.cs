using ClaudeApi.Tools;
using System.ComponentModel;

namespace ClaudeApi.Agents.Tools
{
    public class PromptCacheTools(ISandboxFileManager sandboxFileManager)
    {
        [Tool("load_file_to_cache")]
        [Description("Caches the given array of text file names and caches their contents for later use. Do not cache non-text files!  Cached items will be references in the system prompt.  ###Cached data ###\\n.")]
        public static ToolInvokeResult LoadFilesToPromptCache(string[] fileNames)
        {
            return new ToolInvokeResult("Success", (client, history) =>
            {
                foreach (var fileName in fileNames)
                {
                    client.AddContextFile(fileName);
                }
            });
        }

        [Tool("get_file_names_for_path")]
        [Description("Gets the file names under the specified path. Useful for listing files in a directory.")]
        public string GetFileNamesForPath(string relativePath, bool includeSubdirectories)
        {
            var fileNames = sandboxFileManager.GetFileNamesForPath(relativePath, includeSubdirectories);
            //build a string with the file names
            return string.Join(", ", fileNames);
        }
    }
}
