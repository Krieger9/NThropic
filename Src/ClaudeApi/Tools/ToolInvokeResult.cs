using ClaudeApi.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Tools
{
    // Wraps the result of a tool invocation
    public class ToolInvokeResult
    {
        public object Value { get; set; } = "";

        // Lambda function that takes a Client and List<Message> as parameters
        public Action<Client, List<Message>> SystemCommand { get; set; } = (client, history) => { };

        // Default constructor
        public ToolInvokeResult() { }

        // Constructor to initialize with Value
        public ToolInvokeResult(object value)
        {
            Value = value;
        }

        // Constructor to initialize with SystemCommand
        public ToolInvokeResult(Action<Client, List<Message>> systemCommand)
        {
            SystemCommand = systemCommand;
        }

        // Constructor to initialize with both Value and SystemCommand
        public ToolInvokeResult(object value, Action<Client, List<Message>> systemCommand)
        {
            Value = value;
            SystemCommand = systemCommand;
        }
    }
}
