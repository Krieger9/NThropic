# NThropic

NThropic is a .NET library for accessing Anthropic's Claude API. It's currently a work in progress, focusing on chat completions and tool use integration.

## Features

- Chat completions with Claude API
- Tool use through `ToolAttribute`
- Orchestration agent for managing conversations
- Sandbox environment for file operations
- [Scriban](https://github.com/scriban/scriban) templates for prompts
- Claude's Prompt Caching
- Caching system for System, and Tools.

## Upcoming Features
- Cache two sets of 'Messages'
- Sub-agents for specialized tasks

## Project Structure
- `ClaudeApi`: Core project for API interaction
- `ClaudeApi.Agents`: Sub-agents and features for creating specialist agents
- `CommandLineHost`: Hosts the OrchestrationAgent and demonstrates basic usage
- `Sanctuary`: Currently just the file sandbox code but will be extended to work as the security layer for all external integration channels.

## Simple Chat Agent
CommandLine host will use the Orchestration Agent to perform a simple Chat Agent.  Unfortunately the logging is currently using the same console output.
WinUI3 client coming very soon.  Will likely attempt to split out that difference at some point.
[Orchestration Agent](https://github.com/Krieger9/NThropic/blob/main/Src/ClaudeApi.Agents/OrchestrationAgent.cs)

## Prompts
Prompts can be sent as strings or as [Prompt](https://github.com/Krieger9/NThropic/blob/main/Src/ClaudeApi/Prompts/Prompt.cs) objects.  Prompt objects will load from file.
```
            var prompt = new Prompt("Summarize")
            {
                Arguments = new Dictionary<string, object>
                {
                    { "input", input }
                }
                .Concat(arguments)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            var history = new List<Message>();
            var systemMessage = new List<ContentBlock>();
            var responses = await _client.ProcessContinuousConversationAsync(prompt, history, systemMessage);
```

Prompt parsing takes arguments in a Dictionary.  All parsing is done using the [Scriban library](https://github.com/scriban/scriban).
Currently the file is expected to be a .scriban file.  Will almost certainly add support for .liquid soon due to the ease.

## Tool Usage Example

Tools can be defined using the `ToolAttribute`:
```
public class TestTools
    {
        [Tool("test_echo", "Echoes back the input string")]
        public static string TestEcho(string input)
        {
            return $"Echo: {input}";
        }
    }
```

## Tool Usage Example
Shows basic looping structure for and history management.
[Orchestration Agent](https://github.com/Krieger9/NThropic/blob/main/Src/ClaudeApi.Agents/OrchestrationAgent.cs)

Basic Tool discovery.  This actually loads all tools defined in the same assembly as 'TestTools' type.
```
client.DiscoverTools(typeof(TestTools).Assembly);
```

Currently you can discover tools by...
- All methods marked with the [Tool] attribute in the given assembly.
- All methods marked with the [Tool] attribute in the given type.
- The specific method with the [Tool] attribute in the given type with the specified name.
```
        public void DiscoverTools(Assembly toolAssembly)        
        public void DiscoverTools(Type type)
        public void DiscoverTool(Type type, string methodName)
```


## License

NThropic is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

