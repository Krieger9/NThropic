using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using WinUI3Host.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3Host
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(MainViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            this.rootGrid.DataContext = ViewModel; // Assuming your root element has a name "rootGrid"
        }
    }
}
