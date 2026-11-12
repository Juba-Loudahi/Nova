using Nova.DI.Internal;

namespace Nova.DI;

public sealed class CircularDependencyException : DependencyInjectionException
{
    public CircularDependencyException(IReadOnlyList<Type> path)
        : base(BuildMessage(path))
    {
        Path = Array.AsReadOnly(path.ToArray());
    }

    public IReadOnlyList<Type> Path { get; }

    private static string BuildMessage(IReadOnlyList<Type> path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return $"A circular dependency was detected: {string.Join(" -> ", path.Select(TypeNameHelper.GetDisplayName))}. " +
            "Extract the behaviour these services share into a third service that they can all depend on.";
    }
}
