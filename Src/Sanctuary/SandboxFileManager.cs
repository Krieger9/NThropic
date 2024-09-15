using Microsoft.Extensions.Configuration;
using System.Text;
using System.Configuration;
using ClaudeApi;

namespace Sanctuary
{
    public class SandboxFileManager : ISandboxFileManager
    {
        private readonly string _rootDirectory;
        private readonly int _defaultBufferSize = 81920; // 80KB buffer size

        public SandboxFileManager(IConfiguration configuration)
        {
            _rootDirectory = configuration["Sanctuary:SandBoxFileRoot"]
                ?? throw new InvalidOperationException("Missing Sanctuary:SandBoxFileRoot");

            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }
        }

        public string RootDirectory => _rootDirectory;

        // Asynchronously reads a file and returns a stream
        public Task<Stream> OpenReadAsync(string relativePath)
        {
            var fullPath = ValidatePath(relativePath);
            try
            {
                return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, _defaultBufferSize, useAsync: true));
            }
            catch (Exception ex)
            {
                // Log exception here
                throw new IOException($"Failed to open file for reading: {relativePath}", ex);
            }
        }

        // Asynchronously writes data from a stream to a file
        public async Task WriteAsync(string relativePath, Stream dataStream)
        {
            var fullPath = ValidatePath(relativePath);
            try
            {
                using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, _defaultBufferSize, useAsync: true);
                await dataStream.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                // Log exception here
                throw new IOException($"Failed to write to file: {relativePath}", ex);
            }
        }

        public async IAsyncEnumerable<byte[]> ReadFileInChunksAsync(string relativePath, int bufferSize = 4096)
        {
            var fullPath = ValidatePath(relativePath);
            IAsyncEnumerable<byte[]> chunks;

            try
            {
                chunks = ReadChunksAsync(fullPath, bufferSize);
            }
            catch (Exception ex)
            {
                // Log exception here
                throw new IOException($"Failed to read file in chunks: {relativePath}", ex);
            }

            await foreach (var chunk in chunks)
            {
                yield return chunk;
            }
        }

        private static async IAsyncEnumerable<byte[]> ReadChunksAsync(string fullPath, int bufferSize)
        {
            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
            var buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                yield return buffer[..bytesRead];
            }
        }        // Asynchronously writes chunks of data to a file
        public async Task WriteFileInChunksAsync(string relativePath, IAsyncEnumerable<byte[]> dataChunks)
        {
            var fullPath = ValidatePath(relativePath);
            try
            {
                using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, _defaultBufferSize, useAsync: true);
                await foreach (var chunk in dataChunks)
                {
                    await stream.WriteAsync(chunk);
                }
            }
            catch (Exception ex)
            {
                // Log exception here
                throw new IOException($"Failed to write file in chunks: {relativePath}", ex);
            }
        }

        // Deletes a file
        public void DeleteFile(string relativePath)
        {
            var fullPath = ValidatePath(relativePath);
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                // Log exception here
                throw new IOException($"Failed to delete file: {relativePath}", ex);
            }
        }

        // Checks if a path exists
        public bool PathExists(string relativePath)
        {
            var fullPath = ValidatePath(relativePath);
            return Directory.Exists(fullPath);
        }

        // Checks if a file exists
        public bool FileExists(string relativePath)
        {
            var fullPath = ValidatePath(relativePath);
            return File.Exists(fullPath);
        }

        // Validates and normalizes the provided path
        private string ValidatePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Path cannot be null or empty", nameof(relativePath));
            }

            var combinedPath = Path.Combine(_rootDirectory, relativePath);
            var fullPath = Path.GetFullPath(combinedPath);

            if (!fullPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Access to the path is denied.");
            }

            return fullPath;
        }

        // Checks if the path is safe to access
        public bool PathIsSafe(string path)
        {
            try
            {
                ValidatePath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Generates a string representation of the file structure
        public string GetFileStructure(string relativeRootDir)
        {
            var fullPath = ValidatePath(relativeRootDir);
            var fileStructure = new StringBuilder();
            GenerateFileStructure(fullPath, fileStructure, 0);
            return fileStructure.ToString();
        }

        private static void GenerateFileStructure(string dir, StringBuilder fileStructure, int level)
        {
            string indent = new (' ', 4 * level);
            fileStructure.AppendLine($"{indent}{Path.GetFileName(dir)}/");

            foreach (string subDir in Directory.GetDirectories(dir))
            {
                GenerateFileStructure(subDir, fileStructure, level + 1);
            }

            foreach (string file in Directory.GetFiles(dir))
            {
                fileStructure.AppendLine($"{indent}    {Path.GetFileName(file)}");
            }
        }
    }
}
