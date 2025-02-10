using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SantoriniAI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SantoriniAI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;
        private MainWindowViewModel ViewModel => _viewModel ?? throw new InvalidOperationException("ViewModel is not set");

        public MainWindow()
        {
            this.InitializeComponent();
        }

        public void EnsureWindowDisplay(int width, int height)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Move(new PointInt32(100, 100));
            appWindow.Resize(new SizeInt32(width, height));
            this.Activate();
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DevelopCell("A1");
        }

        internal void SetDataContext(MainWindowViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            _viewModel = viewModel;
            Root.DataContext = ViewModel;
        }
    }
}
