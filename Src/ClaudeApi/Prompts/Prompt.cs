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
        public IDictionary<string, object>? Arguments { get; set; }

        public Prompt(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            Name = name;
        }

        public Prompt Clone(IDictionary<string, object>? arguments = null, bool overwriteArguments = true)
        {
            var clonedArguments = Arguments != null
                ? new Dictionary<string, object>(Arguments)
                : [];

            if (arguments != null)
            {
                foreach (var kvp in arguments)
                {
                    if (overwriteArguments || !clonedArguments.ContainsKey(kvp.Key))
                    {
                        clonedArguments[kvp.Key] = kvp.Value;
                    }
                }
            }

            return new Prompt(Name)
            {
                Arguments = clonedArguments
            };
        }
    }
}
