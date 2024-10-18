using ClaudeApi;
using ClaudeApi.Agents;
using ClaudeApi.Messages;
using ClaudeApi.Prompts;
using System.Text;

namespace CodeAgents.UnitTests
{
    public partial class UnitTestRequirementsAnalyzerAgent(ClaudeClient client, ISandboxFileManager fileManager) : Agent
    {
        internal readonly string FileNameArgumentsKey = "file_name";
        public override async Task<string> ExecuteAsync(string className, Dictionary<string, object> arguments)
        {
            // Verify arguments present
            if (arguments[FileNameArgumentsKey] is not string file_name)
            {
                return "The file name is required";
            }

            // Check if the file exists
            if (!fileManager.FileExists(file_name))
            {
                return "The file does not exist";
            }

            // Read the file contents
            string fileContents;
            using (var stream = await fileManager.OpenReadAsync(file_name))
            using (var reader = new StreamReader(stream))
            {
                fileContents = await reader.ReadToEndAsync();
            }

            // Update the prompt with the file contents
            var prompt = new Prompt("WriteUnitTests")
            {
                Arguments = new Dictionary<string, object>
                    {
                        { "class_name", className },
                        { "code_file_contents", fileContents }
                    }
                .Concat(arguments)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            var history = new List<Message>();
            List<ContentBlock> systemMessage = [ContentBlock.FromString(SystemPrompt)];
            var responses = await client.ProcessContinuousConversationAsync(prompt, history, systemMessage);

            var resultBuilder = new StringBuilder();

            await foreach (var response in responses)
            {
                if (response != null)
                {
                    resultBuilder.Append(response);
                }
            }

            return resultBuilder.ToString();
        }
    }
}
