using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using WinUI3Host.ViewModels;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3Host.Views.Controls
{
    public sealed partial class ChatControl : UserControl
    {
        public IChatViewModel? ChatData { get; set; }
        public ChatControl()
        {
            this.InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
                if (shiftState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                {
                    // Shift+Enter: Add a new line
                    if (sender is TextBox textBox)
                    {
                        int caretIndex = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Insert(caretIndex, "\n");
                        textBox.SelectionStart = caretIndex + 1;
                    }
                }
                else
                {
                    // Enter: Send the message
                    e.Handled = true;
                    if (ChatData != null && ChatData.SendMessageCommand.CanExecute())
                    {
                        ChatData.SendMessageCommand.Execute(R3.Unit.Default);
                    }
                }
            }
        }
    }
}
