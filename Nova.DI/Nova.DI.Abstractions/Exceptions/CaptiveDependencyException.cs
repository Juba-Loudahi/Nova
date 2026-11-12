using Nova.DI.Internal;

namespace Nova.DI;

public sealed class CaptiveDependencyException : DependencyInjectionException
{
    public CaptiveDependencyException(Type singleton, Type scoped)
        : base(BuildMessage(singleton, scoped))
    {
        SingletonType = singleton;
        ScopedType = scoped;
    }

    public Type SingletonType { get; }

    public Type ScopedType { get; }

    private static string BuildMessage(Type singleton, Type scoped)
    {
        ArgumentNullException.ThrowIfNull(singleton);
        ArgumentNullException.ThrowIfNull(scoped);

        string singletonName = TypeNameHelper.GetDisplayName(singleton);
        return $"Captive dependency: the singleton '{singletonName}' depends (directly or not) on the scoped service " +
            $"'{TypeNameHelper.GetDisplayName(scoped)}', which would then outlive its scope. " +
            $"Register '{singletonName}' as scoped, or inject IServiceScopeFactory and create a scope per operation.";
    }
}
