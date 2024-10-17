using ClaudeApi;
using ClaudeApi.Agents;
using ClaudeApi.Agents.DependencyInjection;
using ClaudeApi.DependencyInjection;
using ClaudeApi.Messages;
using ClaudeApi.Services;
using ClaudeApi.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Sanctuary;
using System;
using WinUI3Host.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3Host
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        public App()
        {
            this.InitializeComponent();
            Services = ConfigureServices();
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<App>()
                .Build();

            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);
            services.AddHttpClient<IClaudeApiService, ClaudeApiService>();

            // Register your services and view models here
            services.AddSingleton<IServiceProvider>(serviceProvider => serviceProvider);
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IReactiveUserInterface>(serviceProvider => serviceProvider.GetRequiredService<MainViewModel>());
            services.AddTransient<MainWindow>();
            services.AddTransient<ObservableOrchestrationAgent>();
            services.AddTransient<IToolRegistry, ToolRegistry>();
            services.AddTransient<IPromptService, PromptService>();
            services.AddTransient<ISandboxFileManager, SandboxFileManager>();
            services.AddLogging();
            services.AddTransient<ClaudeClient>()
                .AddClaudApi()
                .AddNThropicAgents();

            // Register additional dependencies for MainViewModel as transient
            services.AddTransient<IChatViewModel, ChatViewModel>();
            services.AddTransient<IUsageStatsViewModel, UsageStatsViewModel>();
            services.AddTransient<IFilesListViewModel, FilesListViewModel>();
            services.AddTransient<Usage>();

            return services.BuildServiceProvider();
        }

        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                var mainWindow = Services.GetRequiredService<MainWindow>();
                mainWindow.Activate();

                var orchestrationAgent = Services.GetRequiredService<ObservableOrchestrationAgent>();
                await orchestrationAgent.StartConversationAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
