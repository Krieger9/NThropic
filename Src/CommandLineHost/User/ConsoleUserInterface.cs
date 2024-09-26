using ClaudeApi.Agents;
using System.Text;

namespace CommandLineHost
{
    internal class ConsoleUserInterface : IUserInterface
    {
        private readonly StringBuilder _partialMessageBuilder = new ();

        public Task<string> PromptAsync(string message)
        {
            Console.WriteLine(message);
            ClearKeyboardBuffer();
            return Task.FromResult(Console.ReadLine() ?? "");
        }

        public void Message(string message)
        {
            Console.WriteLine(message);
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
