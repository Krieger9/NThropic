
using ClaudeApi;
using ClaudeApi.Agents;
using ClaudeApi.Agents.DependencyInjection;
using ClaudeApi.DependencyInjection;
using ClaudeApi.Services;
using ClaudeApi.Tools;
using CommandLineHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sanctuary;

class Program
{
    static async Task Main()
    {
        // Create a service collection and configure our services
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        // Build the service provider
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Register the service provider itself
        serviceCollection.AddSingleton<IServiceProvider>(serviceProvider);

        // Get the ChatBot instance from the service provider
        var chatBot = serviceProvider.GetRequiredService<ChatOrchestrator>();

        // Start the conversation
        await chatBot.StartConversationAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        // Add logging
        services.AddLogging(configure => configure.AddConsole());

        // Register configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Register dependencies
        services.AddHttpClient<IClaudeApiService, ClaudeApiService>();
        services.AddSingleton<ISandboxFileManager, SandboxFileManager>();
        services.AddSingleton<IUserInterface, ConsoleUserInterface>();
        services.AddSingleton<ChatOrchestrator>();
        services.AddTransient<IToolRegistry, ToolRegistry>();
        services.AddTransient<IPromptService, PromptService>();
        services.AddClaudApi();

        services.AddSingleton<ClaudeClient>(serviceProvider => new ClaudeClient(
            serviceProvider.GetRequiredService<ISandboxFileManager>(),
            serviceProvider.GetRequiredService<IToolManagementService>(),
            serviceProvider.GetRequiredService<IClaudeApiService>(),
            serviceProvider.GetRequiredService<ILogger<ClaudeClient>>(),
            serviceProvider.GetRequiredService<IPromptService>(),
            serviceProvider
        ));

        // Register NThropic agents
        services.AddNThropicAgents(configuration);
        // Optionally, configure other services here
    }
}
