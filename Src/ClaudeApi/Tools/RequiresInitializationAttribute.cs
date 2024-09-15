using System;

namespace ClaudeApi.Tools
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RequiresInitializationAttribute : Attribute
    {
    }
}
