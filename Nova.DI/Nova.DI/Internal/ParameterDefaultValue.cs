using System.Reflection;

namespace Nova.DI.Internal;

internal static class ParameterDefaultValue
{
    public static bool TryGet(ParameterInfo parameter, out object? value)
    {
        if (!parameter.HasDefaultValue)
        {
            value = null;
            return false;
        }

        value = parameter.DefaultValue;
        Type type = parameter.ParameterType;
        Type? nullableUnderlying = Nullable.GetUnderlyingType(type);
        Type underlying = nullableUnderlying ?? type;

        if (value is null)
        {
            if (type.IsValueType && nullableUnderlying is null)
            {
                value = Activator.CreateInstance(type);
            }
        }
        else if (underlying.IsEnum)
        {
            value = Enum.ToObject(underlying, value);
        }

        return true;
    }
}
