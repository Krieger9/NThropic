using ClaudeApi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Tools
{
    public class TestTools
    {
        [Tool("test_echo", "Echoes back the input string")]
        public static string TestEcho(string input)
        {
            return $"Echo: {input}";
        }
    }
}
