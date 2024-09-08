using System;
using System.Reflection;

public static class ParameterDefaultValueHelper
{
    public static bool TryGetDefaultValue(ParameterInfo parameter, out object? defaultValue)
    {
        defaultValue = null;

        if (parameter == null)
            return false;

        // Check if the parameter is optional
        if (parameter.IsOptional)
        {
            // For reference types and nullable value types, DBNull.Value indicates a null default
            if (parameter.DefaultValue == DBNull.Value)
            {
                defaultValue = null;
                return true;
            }

            defaultValue = parameter.DefaultValue;
            return true;
        }

        // Handle value types
        if (parameter.ParameterType.IsValueType)
        {
            defaultValue = Activator.CreateInstance(parameter.ParameterType);
            return true;
        }

        return false;
    }
}