
using ClaudeApi;
using ClaudeApi.Agents;
using ClaudeApi.Agents.DependencyInjection;
using ClaudeApi.Agents.Tools;
using ClaudeApi.Agents.User;
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
        var chatBot = serviceProvider.GetRequiredService<OrchestrationAgent>();

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
        services.AddSingleton<Client>(serviceProvider => new Client(
            serviceProvider.GetRequiredService<ISandboxFileManager>(),
            serviceProvider.GetRequiredService<IConfiguration>(),
            serviceProvider.GetRequiredService<ILogger<Client>>(),
            typeof(TestTools).Assembly,
            serviceProvider
        ));
        services.AddSingleton<IUserInterface, ConsoleUserInterface>();
        services.AddSingleton<OrchestrationAgent>();

        // Register NThropic agents
        var client = services.BuildServiceProvider().GetRequiredService<Client>();
        services.AddNThropicAgents(client);

        // Register ISandboxFileManager
        services.AddSingleton<ISandboxFileManager, SandboxFileManager>();

        // Optionally, configure other services here
    }
}
