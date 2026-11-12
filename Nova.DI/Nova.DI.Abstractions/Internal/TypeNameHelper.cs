namespace Nova.DI.Internal;

internal static class TypeNameHelper
{
    public static string GetDisplayName(Type type)
    {
        if (type.IsArray)
        {
            return GetDisplayName(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string name = type.Name;
        int arityIndex = name.IndexOf('`', StringComparison.Ordinal);
        if (arityIndex >= 0)
        {
            name = name[..arityIndex];
        }

        Type[] arguments = type.GetGenericArguments();
        if (type.IsGenericTypeDefinition)
        {
            return name + "<" + new string(',', arguments.Length - 1) + ">";
        }

        return name + "<" + string.Join(", ", arguments.Select(GetDisplayName)) + ">";
    }
}
