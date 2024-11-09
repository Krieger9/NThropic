using ClaudeApi.Agents.Agents;
using ClaudeApi.Agents.Agents.Converters;
using ClaudeApi.Prompts;
using ClaudeApi.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ClaudeApi.Agents.Orchestrations
{
    public class RequestExecutor : IRequestExecutor
    {
        private readonly IConfiguration _configuration;
        //This value sets the initial default value, but it can be changed so new IExecutables will default to the new value.
        private static CHALLENGE_LEVEL _defaultChallengeLevel = CHALLENGE_LEVEL.PROFESSIONAL;

        // State items.  These are what need to be dealt with to make thread safe.
        private readonly ObservableCollection<List<ExecutableResponse>> _executables = [];
        private readonly Dictionary<string, object> _baseArguments = [];
        // This sets the default for new IExecutables, it can change so not directly aligned with field _defaultChallengeLevel
        public CHALLENGE_LEVEL DefaultChallengeLevel { get; set; } = _defaultChallengeLevel;

        private readonly IConverterAgent _converterAgent;
        private readonly IChallengeLevelAssesementAgent _challengeLevelAssesementAgent;
        private readonly ISmartClient _client;
        private readonly IPromptService _promptService;
        private readonly IServiceProvider _serviceProvider;

        public string Contents { get { return GenerateContentsString(); } }
        public IConverterAgent ConverterAgent { get { return _converterAgent; } }
        public IChallengeLevelAssesementAgent ChallengeLevelAssesementAgent { get { return _challengeLevelAssesementAgent; } }
        public ISmartClient Client { get { return _client; } }
        public IPromptService PromptService { get { return _promptService; } }
        public IServiceProvider ServiceProvider { get { return _serviceProvider; } }

        private string GenerateContentsString()
        {
            return GenerateReport();
        }

        public RequestExecutor(IConfiguration configuration,
            IConverterAgent genericConverterAgent,
            IChallengeLevelAssesementAgent challengeLevelAssesementAgent,
            ISmartClient client,
            IPromptService promptService,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _challengeLevelAssesementAgent = challengeLevelAssesementAgent;
            _client = client;
            _converterAgent = genericConverterAgent;
            _promptService = promptService;
            _serviceProvider = serviceProvider;

            var defaultChallengeLevel = _configuration.GetValue<string>("DefaultChallengeLevel");
            if (!string.IsNullOrEmpty(defaultChallengeLevel))
            {
                _defaultChallengeLevel = Enum.Parse<CHALLENGE_LEVEL>(defaultChallengeLevel);
            }
        }

        public IRequestExecutor Ask(string ask, CHALLENGE_LEVEL? challengeLevel = null)
        {
            return Ask(new List<string> { ask }, challengeLevel);
        }

        public IRequestExecutor Ask(List<string> asks, CHALLENGE_LEVEL? challengeLevel = null)
        {
            challengeLevel ??= _defaultChallengeLevel;
            var asksList = asks.Select(s => new ExecutableResponse(new Ask { Prompt = s, ChallengeLevel = challengeLevel.Value }));
            if (_executables.Count > 0)
            {
                _executables.Last().AddRange(asksList);
            }
            else
            {
                _executables.Add(asksList.ToList());
            }
            return this;
        }

        public IRequestExecutor ThenAsk(string ask, CHALLENGE_LEVEL? challengeLevel = null)
        {
            return ThenAsk(new List<string> { ask }, challengeLevel);
        }

        public IRequestExecutor ThenAsk(List<string> asks, CHALLENGE_LEVEL? challengeLevel = null)
        {
            challengeLevel ??= _defaultChallengeLevel;
            _executables.Add(asks.Select(s => new ExecutableResponse(new Ask { Prompt = s, ChallengeLevel = challengeLevel.Value })).ToList());
            return this;
        }

        public IRequestExecutor Ask(Prompt prompt, CHALLENGE_LEVEL? challengeLevel = null)
        {
            return Ask(new List<Prompt> { prompt }, challengeLevel);
        }

        public IRequestExecutor Ask(List<Prompt> prompts, CHALLENGE_LEVEL? challengeLevel = null)
        {
            challengeLevel ??= _defaultChallengeLevel;
            _executables.Add(prompts.Select(p => new ExecutableResponse(new PromptAsk { Prompt = p, ChallengeLevel = challengeLevel.Value })).ToList());
            return this;
        }

        public IRequestExecutor ThenAsk(Prompt prompt, CHALLENGE_LEVEL? challengeLevel = null)
        {
            return ThenAsk(new List<Prompt> { prompt }, challengeLevel);
        }

        public IRequestExecutor ThenAsk(List<Prompt> prompts, CHALLENGE_LEVEL? challengeLevel = null)
        {
            challengeLevel ??= _defaultChallengeLevel;
            _executables.Add(prompts.Select(p => new ExecutableResponse(new PromptAsk { Prompt = p, ChallengeLevel = challengeLevel.Value })).ToList());
            return this;
        }

        public IRequestExecutor ProcessByAgent(IAgent agent)
        {
            _executables.Add([new(new AgentExecutable { Agent = agent })]);
            return this;
        }

        public IRequestExecutor ConvertTo<T>()
        {
            _executables.Add([new(new ConvertTo<T>())]);
            return this;
        }

        public async Task<IRequestExecutor> ExecuteAsync()
        {
            foreach (var executableList in _executables)
            {
                var tasks = executableList.Select(async executableResponse =>
                {
                    var response = await executableResponse.Executable.ExecuteAsync(this);
                    if (executableResponse.Executable is IObjectValue objectValueItem)
                    {
                        executableResponse.Response = objectValueItem.ObjectValue;
                    }
                    else
                    {
                        executableResponse.Response = response;
                    }
                    return executableResponse;
                }).ToList();

                var responses = await Task.WhenAll(tasks);
            }

            return this;
        }

        public Task<T?> AsAsync<T>()
        {
            var item = _executables.Last().First().Response;
            if(item is null) return Task.FromResult(default(T));
            if (item is T t)
            {
                return Task.FromResult<T?>(t);
            }
            else
            {
                throw new InvalidCastException($"Cannot cast {item.GetType()} to {typeof(T)}");
            }
        }

        public string GenerateReport()
        {
            var reportBuilder = new StringBuilder();
            int setCounter = 1;

            foreach (var executableList in _executables)
            {
                reportBuilder.AppendLine($"Set {setCounter}:");

                foreach (var executableResponse in executableList)
                {
                    reportBuilder.AppendLine($"  Question: {executableResponse.Executable}");
                    reportBuilder.AppendLine($"  Answer: {executableResponse.Response}");
                    reportBuilder.AppendLine();
                }

                setCounter++;
            }

            return reportBuilder.ToString();
        }

        public void Clear()
        {
            ClearArguments();
            ClearExecutables();
        }

        public void ClearExecutables()
        {
            _executables.Clear();
        }

        public void ClearArguments()
        {
            _baseArguments.Clear();
        }

        public IRequestExecutor AddArguments(Dictionary<string, object> addArgs)
        {
            // append the new arguments to the base arguments
            foreach (var arg in addArgs)
            {
                _baseArguments[arg.Key] = arg.Value;
            }
            return this;
        }

        public IRequestExecutor Contextualize()
        {
            throw new NotImplementedException();
        }
    }
}
