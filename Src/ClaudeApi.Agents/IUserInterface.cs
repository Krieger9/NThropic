using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudiaCore
{
    public interface IUserInterface
    {
        void AddArtifact(string artifact);
        string Prompt(string message);
        void Message(string message);
        void ReceivePartialMessage(string partialMessage);
        void EndPartialMessage();
    }
}
