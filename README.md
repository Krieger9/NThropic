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


## License

NThropic is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

