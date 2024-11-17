using ClaudeApi.Agents.Agents;
using ClaudeApi.Agents.Agents.Converters;
using ClaudeApi.Prompts;
using ClaudeApi.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
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

        public IConverterAgent ConverterAgent { get { return _converterAgent; } }
        public IChallengeLevelAssesementAgent ChallengeLevelAssesementAgent { get { return _challengeLevelAssesementAgent; } }
        public ISmartClient Client { get { return _client; } }
        public IPromptService PromptService { get { return _promptService; } }
        public IServiceProvider ServiceProvider { get { return _serviceProvider; } }
        public IDictionary<string, object> BaseArguments
        {
            get { return new ReadOnlyDictionary<string, object>(_baseArguments); }
        }
        public IRequestExecutor SetChallengeLevel(CHALLENGE_LEVEL challengeLevel)
        {
            DefaultChallengeLevel = challengeLevel;
            return this;
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
            return Ask([ask], challengeLevel);
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
            return ThenAsk([ask], challengeLevel);
        }

        public IRequestExecutor ThenAsk(List<string> asks, CHALLENGE_LEVEL? challengeLevel = null)
        {
            challengeLevel ??= _defaultChallengeLevel;
            _executables.Add(asks.Select(s => new ExecutableResponse(new Ask { Prompt = s, ChallengeLevel = challengeLevel.Value })).ToList());
            return this;
        }

        public IRequestExecutor Ask(Prompt prompt, Dictionary<string, object>? arguments, CHALLENGE_LEVEL? challengeLevel = null)
        {
            return Ask([prompt], arguments, challengeLevel);
        }

        public IRequestExecutor Ask(List<Prompt> prompts, Dictionary<string, object>? arguments, CHALLENGE_LEVEL? challengeLevel = null)
        {
            _executables.Add(
                prompts.Select(p =>
                {
                    return new ExecutableResponse(
                        new PromptAsk { Prompt = p, RunArguments = arguments, ChallengeLevel = challengeLevel });
                }).ToList());
            return this;
        }

        public IRequestExecutor ThenAsk(Prompt prompt, Dictionary<string, object>? arguments = null, CHALLENGE_LEVEL? challengeLevel = null)
        {
            return ThenAsk([prompt], arguments, challengeLevel);
        }

        public IRequestExecutor ThenAsk(List<Prompt> prompts, Dictionary<string, object>? arguments = null, CHALLENGE_LEVEL? challengeLevel = null)
        {
            _executables.Add(
                prompts.Select(p =>
                {
                    return new ExecutableResponse(new PromptAsk { Prompt = p, RunArguments = arguments, ChallengeLevel = challengeLevel });
                }).ToList());
            return this;
        }

        public IRequestExecutor ProcessByAgent(IAgent agent)
        {
            _executables.Add([new(new AgentExecutable { Agent = agent })]);
            return this;
        }

        public async Task<string> Result()
        {
            await ExecuteAsync();
            return string.Join("\n", _executables.Last().Select(r => r.Response));
        }

        public async Task<T> ConvertTo<T>()
        {
            var result = await ExecuteAsync();
            var prompt = string.Join("\n", result.Information);
            var converted_object = await _converterAgent.ConvertToAsync(prompt, typeof(T));
            if (converted_object is T t)
            {
                return t;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast {converted_object?.GetType()} to {typeof(T)}");
            }
        }

        public async Task<IRequestExecutor> ExecuteAsync()
        {
            ConcurrentBag<Exception> exceptions = [];

            foreach (var executableList in _executables)
            {
                var tasks = executableList.Select(async executableResponse =>
                {
                    var response = "";
                    try
                    {
                        response = await executableResponse.Executable.ExecuteAsync(this);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
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
                if (!exceptions.IsEmpty)
                {
                    throw new AggregateException(exceptions);
                }
            }

            return this;
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

        public string InformationString { get { return string.Join("\n", Information); } }
        public List<string> Information
        {
            get
            {
                var responses = new List<string>();

                foreach (var executableList in _executables)
                {
                    foreach (var executableResponse in executableList)
                    {
                        var asString = executableResponse.Response?.ToString();
                        if (executableResponse.Response != null 
                            && executableResponse.Response is not IObjectValue
                            && !string.IsNullOrWhiteSpace(asString))
                        {
                            responses.Add(asString);
                        }
                    }
                }

                return responses;
            }
        }
    }
}
