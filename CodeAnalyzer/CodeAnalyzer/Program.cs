// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using ClaudeApi.Agents.Orchestrations;
using CodeAnalyzer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ClaudeApi.Agents.Agents.Converters;
using ClaudeApi;
using Sanctuary;
using ClaudeApi.Agents.DependencyInjection;
using ClaudeApi.DependencyInjection;

var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);

var serviceProvider = serviceCollection.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

if (args.Length != 2)
{
    logger.LogError("Usage: CodeAnalyzer <inputFilePath> <outputFilePath>");
    return;
}

string inputFilePath = args[0].Trim(['\'', '\"']);
string outputFilePath = args[1].Trim(['\'', '\"']);

if (!File.Exists(inputFilePath))
{
    logger.LogError("Error: The file '{InputFilePath}' does not exist.", inputFilePath);
    return;
}

if (string.IsNullOrEmpty(outputFilePath))
{
    logger.LogError("Error: The output file path is required.");
    return;
}

string? outputDirectory = Path.GetDirectoryName(outputFilePath);
if (!Directory.Exists(outputDirectory))
{
    logger.LogError("Error: The directory '{OutputDirectory}' does not exist.", outputDirectory);
    return;
}

var analyzer = serviceProvider.GetService<IFileAnalyzer>();
if (analyzer == null)
{
    logger.LogError("Error: The file analyzer could not be created.");
    return;
}

await analyzer.AnalyzeAsync(inputFilePath, outputFilePath);

static void ConfigureServices(IServiceCollection services)
{
    // Build configuration
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("Application.json", optional: false, reloadOnChange: true)
        .AddUserSecrets<Program>() 
        .Build();

    // Register configuration
    services.AddSingleton<IConfiguration>(configuration);

    // Register logging
    services.AddLogging(configure => configure.AddConsole());

    services.AddTransient<IFileAnalyzer, FileAnalyzer>();
    services.AddTransient<IRequestExecutor, RequestExecutor>();
    services.AddTransient<IConverterAgent, GenericConverterAgent>();
    services.AddTransient<ISandboxFileManager, SandboxFileManager>();
    services.AddHttpClient();
    services.AddClaudApi();
    services.AddNThropicAgents(configuration);
}
