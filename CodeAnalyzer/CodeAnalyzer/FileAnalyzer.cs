using ClaudeApi.Agents.Extensions;
using ClaudeApi.Agents.Orchestrations;
using ClaudeApi.Prompts;
using System;
using System.ComponentModel;
using System.IO;

namespace CodeAnalyzer
{
public class FileAnalyzer(IRequestExecutor executor) : IFileAnalyzer
    {
        [Description("File Types Enum")]
        public enum FileTypes
        {
            [Description("An empty file")]
            Empty,
            [Description("Text file")]
            Text,
            [Description("Code file")]
            Code,
            [Description("Binary file")]
            Binary,
            [Description("Other file type")]
            Other
        }

        public async Task AnalyzeAsync(string inputFilePath, string outputFilePath)
        {
            try
            {
                using FileStream inputStream = new(inputFilePath, FileMode.Open, FileAccess.Read);
                Console.WriteLine("Analyzing the file...");
                var run = await executor
                    .Ask(new Prompt("GetFileType")
                    {
                        Name = "GetFileType",
                        Arguments = new Dictionary<string, object> { 
                            { "file_contents", await new StreamReader(inputStream).ReadToEndAsync() },
                            { "possible_types", default(FileTypes).GetEnumDescription() }
                        }
                    })
                    .ConvertTo<FileTypes>()
                    .ExecuteAsync();
                var file_type = await run.AsAsync<FileTypes>();

                if (file_type == FileTypes.Text)
                {
                    Console.WriteLine("The file is a text file.");
                }
                else if (file_type == FileTypes.Code)
                {
                    Console.WriteLine("The file is a code file.");
                }
                else if (file_type == FileTypes.Binary)
                {
                    Console.WriteLine("The file is a binary file.");
                }
                else
                {
                    Console.WriteLine("The file is of an unknown type.");
                }

                File.WriteAllText(outputFilePath, run.Contents);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during file analysis: {ex.Message}");
            }
        }
    }
}
