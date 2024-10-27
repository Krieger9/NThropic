using System;
using System.IO;

namespace CodeAnalyzer
{
    public interface IFileAnalyzer
    {
        Task AnalyzeAsync(string inputFilePath, string outputFilePath);
    }
}