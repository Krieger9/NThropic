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
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (Name?.GetHashCode() ?? 0);
            if (Arguments != null)
            {
                foreach (var kvp in Arguments)
                {
                    hash = hash * 23 + kvp.Key.GetHashCode();
                    hash = hash * 23 + (kvp.Value?.GetHashCode() ?? 0);
                }
            }
            return hash;
        }
        public override bool Equals(object? obj)
        {
            if (obj is Prompt other)
            {
                if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
                {
                    return false;
                }

                if (Arguments == null && other.Arguments == null)
                {
                    return true;
                }

                if (Arguments == null || other.Arguments == null)
                {
                    return false;
                }

                if (Arguments.Count != other.Arguments.Count)
                {
                    return false;
                }

                foreach (var kvp in Arguments)
                {
                    if (!other.Arguments.TryGetValue(kvp.Key, out var otherValue) ||
                        !Equals(kvp.Value, otherValue))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
