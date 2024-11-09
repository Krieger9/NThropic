using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ClaudeApi.Agents.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription<T>(this T _) where T : Enum
        {
            var enumType = typeof(T);
            var enumDescription = enumType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? enumType.Name;
            var descriptions = new List<string> { $"enum Description: {enumDescription}\n\nEnumeration:Description\n" };

            foreach (var value in Enum.GetValues(enumType))
            {
                var fieldInfo = enumType.GetField(value.ToString()!);
                if (fieldInfo == null)
                {
                    continue;
                }

                var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                var description = descriptionAttribute != null
                    ? descriptionAttribute.Description
                    : value.ToString();

                descriptions.Add($"{value}: {description}");
            }

            return string.Join(Environment.NewLine, descriptions);
        }
    }
}
