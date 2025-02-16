using ClaudeApi.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SantoriniAI.Controls
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
