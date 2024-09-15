using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ClaudeApi
{
    public interface ISandboxFileManager
    {
        string RootDirectory { get; }
        Task<Stream> OpenReadAsync(string relativePath);
        Task WriteAsync(string relativePath, Stream dataStream);
        IAsyncEnumerable<byte[]> ReadFileInChunksAsync(string relativePath, int bufferSize = 4096);
        Task WriteFileInChunksAsync(string relativePath, IAsyncEnumerable<byte[]> dataChunks);
        void DeleteFile(string relativePath);
        bool PathExists(string relativePath);
        bool FileExists(string relativePath);
        bool PathIsSafe(string path);
        string GetFileStructure(string relativeRootDir);
    }
}
