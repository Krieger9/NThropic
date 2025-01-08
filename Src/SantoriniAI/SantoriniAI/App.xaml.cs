using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SantoriniAI
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
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<App>()
                .Build();

            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Register your services and view models here
            services.AddSingleton<IServiceProvider>(serviceProvider => serviceProvider);
            services.AddTransient<MainWindow>();
            services.AddLogging();
           
            return services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                var mainWindow = Services.GetRequiredService<MainWindow>();
                mainWindow.Activate();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
