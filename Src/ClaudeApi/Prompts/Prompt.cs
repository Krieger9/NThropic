using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Prompts
{
    public class Prompt
    {
        public string Name { get; set; }
        public Dictionary<string, object>? Arguments { get; set; }

        public Prompt(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            Name = name;
        }
    }
}
