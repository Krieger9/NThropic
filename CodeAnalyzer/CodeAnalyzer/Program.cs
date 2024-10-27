// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using ClaudeApi.Agents.Orchestrations;
using CodeAnalyzer;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);

var serviceProvider = serviceCollection.BuildServiceProvider();

if (args.Length != 2)
{
    Console.WriteLine("Usage: CodeAnalyzer <inputFilePath> <outputFilePath>");
    return;
}

string inputFilePath = args[0];
string outputFilePath = args[1];

if (!File.Exists(inputFilePath))
{
    Console.WriteLine($"Error: The file '{inputFilePath}' does not exist.");
    return;
}

if (string.IsNullOrEmpty(outputFilePath))
{ 
    Console.WriteLine("Error: The output file path is required."); 
    return; 
}

string? outputDirectory = Path.GetDirectoryName(outputFilePath);
if (!Directory.Exists(outputDirectory))
{
    Console.WriteLine($"Error: The directory '{outputDirectory}' does not exist.");
    return;
}

var analyzer = serviceProvider.GetService<IFileAnalyzer>();
if (analyzer == null)
{
    Console.WriteLine("Error: The file analyzer could not be created.");
    return;
}

await analyzer.AnalyzeAsync(inputFilePath, outputFilePath);

static void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IFileAnalyzer, FileAnalyzer>();
    services.AddTransient<IRequestExecutor, RequestExecutor>();
}
