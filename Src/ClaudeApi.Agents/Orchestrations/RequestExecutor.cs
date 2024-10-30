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
        private static CHALLENGE_LEVEL _defaultChallengeLevel = CHALLENGE_LEVEL.PROFESSIONAL;

        private readonly ObservableCollection<List<ExecutableResponse>> _executables = [];
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

        public IRequestExecutor Ask(string ask)
        {
            return Ask([ask]);
        }

        public IRequestExecutor Ask(List<string> asks)
        {
            return Ask(asks, _defaultChallengeLevel);
        }

        public IRequestExecutor Ask(List<string> prompts, CHALLENGE_LEVEL challengeLevel = CHALLENGE_LEVEL.NONE)
        {
            if (challengeLevel == CHALLENGE_LEVEL.NONE)
            {
                challengeLevel = _defaultChallengeLevel;
            }
            var asks = prompts.Select(s => new ExecutableResponse(new Ask { Prompt = s, ChallengeLevel = challengeLevel }));
            if (_executables.Count > 0)
            {
                _executables.Last().AddRange(asks);
            }
            else
            {
                _executables.Add(asks.ToList());
            }
            return this;
        }

        public IRequestExecutor ThenAsk(string ask)
        {
            return ThenAsk([ask]);
        }

        public IRequestExecutor ThenAsk(List<string> asks)
        {
            _executables.Add(asks.Select(s => new ExecutableResponse(new Ask { Prompt = s, ChallengeLevel = _defaultChallengeLevel })).ToList());
            return this;
        }

        public IRequestExecutor Ask(Prompt prompt)
        {
            return Ask([prompt]);
        }

        public IRequestExecutor Ask(List<Prompt> prompts)
        {
            _executables.Add(prompts.Select(p => new ExecutableResponse(new PromptAsk { Prompt = p, ChallengeLevel = _defaultChallengeLevel })).ToList());
            return this;
        }

        public IRequestExecutor ThenAsk(Prompt prompt)
        {
            _executables.Add([new ExecutableResponse(new PromptAsk { Prompt = prompt, ChallengeLevel = _defaultChallengeLevel })]);
            return this;
        }

        public IRequestExecutor ThenAsk(List<Prompt> prompts)
        {
            _executables.Add(prompts.Select(p => new ExecutableResponse(new PromptAsk { Prompt = p, ChallengeLevel = _defaultChallengeLevel })).ToList());
            return this;
        }

        public IRequestExecutor ProcessByAgent(IAgent agent)
        {
            _executables.Add([new ExecutableResponse(new AgentExecutable { Agent = agent })]);
            return this;
        }

        public IRequestExecutor ConvertTo<T>()
        {
            _executables.Add([new ExecutableResponse(new ConvertTo<T>())]);
            return this;
        }

        public async Task<IRequestExecutor> ExecuteAsync()
        {
            foreach (var executableList in _executables)
            {
                var tasks = executableList.Select(async executableResponse =>
                {
                    var response = await executableResponse.Executable.ExecuteAsync(this);
                    executableResponse.Response = response;
                    return executableResponse;
                }).ToList();

                var responses = await Task.WhenAll(tasks);
            }

            return this;
        }

        public Task<T?> AsAsync<T>()
        {
            var item = _executables.Last().First().Response;
            if(item is T t)
            {
                return Task.FromResult<T?>(t);
            }
            else
            {
                return Task.FromResult(default(T));
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
                    reportBuilder.AppendLine($"  Question: {((Ask)executableResponse.Executable).Prompt}");
                    reportBuilder.AppendLine($"  Answer: {executableResponse.Response}");
                    reportBuilder.AppendLine();
                }

                setCounter++;
            }

            return reportBuilder.ToString();
        }

    }
}
