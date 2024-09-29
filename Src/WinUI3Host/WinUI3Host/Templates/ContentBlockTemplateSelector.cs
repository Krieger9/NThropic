using ClaudeApi.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI3Host.Templates
{
    public class ContentBlockTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextContentTemplate { get; set; }
        public DataTemplate UserMessageTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TextContentBlock)
            {
                return TextContentTemplate;
            }

            // Assuming your message class is named 'Message'
            if (item is Message message && message.Role == "User")
            {
                return UserMessageTemplate;
            }
            // Add more conditions here for other ContentBlock types in the future

            return base.SelectTemplateCore(item, container);
        }
    }
}
