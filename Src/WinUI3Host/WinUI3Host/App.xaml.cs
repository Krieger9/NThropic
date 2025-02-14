﻿using ClaudeApi;
using ClaudeApi.Agents;
using ClaudeApi.Agents.ChatTracking;
using ClaudeApi.Agents.ChatTracking.OpenTelemetry;
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
using WinUI3Host.Telemetry;
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
        private const string AppNameKey = "AppSettings:AppName";
        private const string TelemetryConnectionStringKey = "Telemetry:ConnectionString";

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

            // Retrieve appName and telemetryInstrumentationKey from configuration
            var appName = configuration[AppNameKey];
            var telemetryInstrumentationKey = configuration[TelemetryConnectionStringKey];

            // Validate configuration values
            if (string.IsNullOrEmpty(appName))
            {
                throw new InvalidOperationException($"{AppNameKey} is not configured.");
            }

            if (string.IsNullOrEmpty(telemetryInstrumentationKey))
            {
                throw new InvalidOperationException($"{TelemetryConnectionStringKey} is not configured.");
            }

            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);
            services.AddHttpClient<IClaudeApiService, ClaudeApiService>();

            // Register your services and view models here
            services.AddSingleton<IServiceProvider>(serviceProvider => serviceProvider);
            TelemetryInitializer.Initialize(appName, telemetryInstrumentationKey);
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IReactiveUserInterface>(serviceProvider => serviceProvider.GetRequiredService<MainViewModel>());
            services.AddTransient<MainWindow>();
            services.AddTransient<ObservableChatOrchestrator>();
            services.AddTransient<IToolRegistry, ToolRegistry>();
            services.AddTransient<IPromptService, PromptService>();
            services.AddTransient<ISandboxFileManager, SandboxFileManager>();
            services.AddLogging();
            services.AddTransient<ClaudeClient>()
                .AddClaudApi()
                .AddNThropicAgents(configuration);

            // Register OpenTelemetryConversationLogger with the appName
            services.AddSingleton<IConversationLogger>(provider => new OpenTelemetryConversationLogger(appName));

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

                var orchestrationAgent = Services.GetRequiredService<ObservableChatOrchestrator>();
                await orchestrationAgent.StartConversationAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
