using ClaudeApi.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace WinUI3Host.Templates
{
    public partial class ContentBlockTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? TextContentTemplate { get; set; }
        public DataTemplate? UserMessageTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return item switch
            {
                TextContentBlock => TextContentTemplate ?? throw new InvalidOperationException("TextContentTemplate is not assigned."),
                Message message when message.Role == "User" => UserMessageTemplate ?? throw new InvalidOperationException("UserMessageTemplate is not assigned."),
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
