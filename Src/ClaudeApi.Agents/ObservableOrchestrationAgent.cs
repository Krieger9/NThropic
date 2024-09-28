using ClaudeApi.Agents.Tools;
using ClaudeApi.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents
{
    public partial class ObservableOrchestrationAgent : OrchestrationAgent
    {
        private readonly IObservableMessageHistory _observableMessageHistory;

        public ObservableOrchestrationAgent(ClaudeClient client, IUserInterface userInterface, IObservableMessageHistory messageHistory)
            : base(client, userInterface, messageHistory)
        {
            _observableMessageHistory = messageHistory;
        }

        public new IObservableMessageHistory MessageHistory => _observableMessageHistory;

        public new event Action<Message>? MessageAdded;

        protected override void OnMessageAdded(Message message)
        {
            MessageAdded?.Invoke(message);
        }

        public override async Task StartConversationAsync()
        {
            await base.StartConversationAsync();
        }
    }
}
