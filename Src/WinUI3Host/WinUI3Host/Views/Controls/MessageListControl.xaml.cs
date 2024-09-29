using ClaudeApi.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3Host.Views.Controls
{
    public sealed partial class MessageListControl : UserControl
    {
        public MessageListControl()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<Message> Messages
        {
            get { return (ObservableCollection<Message>)GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }

        public static readonly DependencyProperty MessagesProperty =
            DependencyProperty.Register("Messages", typeof(List<Message>), typeof(MessageListControl), new PropertyMetadata(new List<Message>()));
    }
}
