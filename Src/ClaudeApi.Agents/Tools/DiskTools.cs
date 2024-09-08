using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ClaudeApi.Tools;

namespace ClaudiaCore.Tools
{
    public static class DiskTools
    {
        private static readonly Lazy<string> _sandboxBasePath;
        private static IConfiguration? _configuration;

        static DiskTools()
        {
            _sandboxBasePath = new Lazy<string>(() =>
            {
                if (_configuration == null)
                    throw new InvalidOperationException("DiskTools has not been initialized with a configuration.");

                var path = _configuration["Sanctuary:SandboxBasePath"];
                if (string.IsNullOrEmpty(path))
                    throw new InvalidOperationException("Sanctuary:SandboxBasePath is not set in the configuration.");

                return path;
            });
        }

        public static string SandboxBasePath => _sandboxBasePath.Value;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private static string GetSafePath(string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(SandboxBasePath, relativePath));
            if (!fullPath.StartsWith(SandboxBasePath, StringComparison.OrdinalIgnoreCase))
                throw new SecurityException("Access to path outside of sandbox is not allowed.");

            return fullPath;
        }

        [Tool("read_file", "Reads the content of a file from the sandbox")]
        public static async Task<string> ReadFileAsync(string relativePath)
        {
            var safePath = GetSafePath(relativePath);
            return await File.ReadAllTextAsync(safePath, Encoding.UTF8);
        }

        [Tool("write_file", "Writes content to a file in the sandbox, creating directories if needed")]
        public static async Task WriteFileAsync(string relativePath, string content)
        {
            var safePath = GetSafePath(relativePath);
            var directoryPath = Path.GetDirectoryName(safePath) ?? throw new InvalidOperationException("The directory path could not be determined.");

            Directory.CreateDirectory(directoryPath);
            await File.WriteAllTextAsync(safePath, content, Encoding.UTF8);
        }

        [Tool("append_to_file", "Appends content to an existing file in the sandbox, creating directories if needed")]
        public static async Task AppendToFileAsync(string relativePath, string content)
        {
            var safePath = GetSafePath(relativePath);
            var directoryPath = Path.GetDirectoryName(safePath) ?? throw new InvalidOperationException("The directory path could not be determined.");

            Directory.CreateDirectory(directoryPath);
            await File.AppendAllTextAsync(safePath, content, Encoding.UTF8);
        }

        [Tool("file_exists", "Checks if a file exists in the sandbox")]
        public static bool FileExists(string relativePath)
        {
            var safePath = GetSafePath(relativePath);
            return File.Exists(safePath);
        }

        [Tool("create_directory", "Creates a new directory in the sandbox")]
        public static async Task CreateDirectoryAsync(string relativePath)
        {
            var safePath = GetSafePath(relativePath);
            await Task.Run(() => Directory.CreateDirectory(safePath));
        }
    }
}
