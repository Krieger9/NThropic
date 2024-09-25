using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ClaudeApi.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace ClaudeApi.Agents.Tools
{
    public class DiskTools
    {
        private readonly ISandboxFileManager _sandboxFileManager;

        public DiskTools(ISandboxFileManager sandboxFileManager)
        {
            _sandboxFileManager = sandboxFileManager ?? throw new ArgumentNullException(nameof(sandboxFileManager));
        }

        [Tool("read_file")]
        [Description("Reads the content of a file from the sandbox")]
        public async Task<string> ReadFileAsync(string relativePath)
        {
            try
            {
                using var stream = await _sandboxFileManager.OpenReadAsync(relativePath);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"UnauthorizedAccessException: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        [Tool("write_file")]
        [Description("Writes content to a file in the sandbox, creating directories if needed")]
        public async Task<string> WriteFileAsync(string relativePath, string content)
        {
            try
            {
                var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                await _sandboxFileManager.WriteAsync(relativePath, dataStream);
                return "File written successfully.";
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"UnauthorizedAccessException: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        [Tool("append_to_file")]
        [Description("Appends content to an existing file in the sandbox, creating directories if needed")]
        public async Task<string> AppendToFileAsync(string relativePath, string content)
        {
            try
            {
                if (!_sandboxFileManager.FileExists(relativePath))
                {
                    return await WriteFileAsync(relativePath, content);
                }

                using var stream = await _sandboxFileManager.OpenReadAsync(relativePath);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var existingContent = await reader.ReadToEndAsync();
                var newContent = existingContent + content;

                return await WriteFileAsync(relativePath, newContent);
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"UnauthorizedAccessException: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        [Tool("file_exists")]
        [Description("Checks if a file exists in the sandbox")]
        public string FileExists(string relativePath)
        {
            try
            {
                return _sandboxFileManager.FileExists(relativePath) ? "File exists." : "File does not exist.";
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"UnauthorizedAccessException: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        [Tool("create_directory")]
        [Description("Creates a new directory in the sandbox")]
        public async Task<string> CreateDirectoryAsync(string relativePath)
        {
            try
            {
                await _sandboxFileManager.CreateDirectoryAsync(relativePath);
                return "Directory created successfully.";
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"UnauthorizedAccessException: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        [Tool("get_directory_tree")]
        public async Task<string> GetDirectoryTreeAsync(string relativePath)
        {
            try
            {
                return await Task.Run(() => _sandboxFileManager.GetFileStructure(relativePath));
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"UnauthorizedAccessException: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
    }
}
