using ClaudeApi.Agents;
using ClaudeApi.Messages;
using System.Collections.ObjectModel;
using System.Text;

namespace CommandLineHost
{
    internal class ConsoleUserInterface : IUserInterface
    {
        private readonly StringBuilder _partialMessageBuilder = new();

        public Task<string> PromptAsync(string message)
        {
            Console.WriteLine(message);
            ClearKeyboardBuffer();
            return Task.FromResult(Console.ReadLine() ?? "");
        }

        public void Message(Message message)
        {
            if (message.Content is ObservableCollection<ContentBlock> contentBlocks)
            {
                foreach (var contentBlock in contentBlocks)
                {
                    if (contentBlock is TextContentBlock textContentBlock)
                    {
                        Console.WriteLine(textContentBlock.Text);
                    }
                    else
                    {
                        Console.WriteLine(contentBlock.ToString());
                    }
                }
            }
            else
            {
                Console.WriteLine(message.ToString());
            }
        }

        public void ReceivePartialMessage(string partialMessage)
        {
            _partialMessageBuilder.Append(partialMessage);
            Console.Write(partialMessage);
        }

        public void EndPartialMessage()
        {
            Console.WriteLine(); // Move to the next line after the partial message is complete
            _partialMessageBuilder.Clear();
        }

        private static void ClearKeyboardBuffer()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
            }
        }
    }
}
