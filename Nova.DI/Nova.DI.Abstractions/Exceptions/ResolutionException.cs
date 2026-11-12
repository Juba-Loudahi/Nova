using Nova.DI.Internal;

namespace Nova.DI;

public sealed class ResolutionException : DependencyInjectionException
{
    public ResolutionException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }

    public static ResolutionException NotRegistered(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        string name = TypeNameHelper.GetDisplayName(serviceType);
        return new ResolutionException(
            $"No service of type '{name}' has been registered. " +
            $"Register it in the service collection, for example with services.AddScoped<{name}, TImplementation>().");
    }

    public static ResolutionException MissingDependency(Type dependency, Type consumer, string path)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(consumer);
        ArgumentNullException.ThrowIfNull(path);

        string dependencyName = TypeNameHelper.GetDisplayName(dependency);
        return new ResolutionException(
            $"Unable to resolve '{dependencyName}', required by the constructor of '{TypeNameHelper.GetDisplayName(consumer)}'. " +
            $"Path: {path}. Make sure '{dependencyName}' is registered.");
    }

    public static ResolutionException NoPublicConstructor(Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(implementationType);

        return new ResolutionException(
            $"'{TypeNameHelper.GetDisplayName(implementationType)}' has no public constructor. " +
            "Add a public constructor or register the service with a factory.");
    }

    public static ResolutionException MultipleInjectionConstructors(Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(implementationType);

        return new ResolutionException(
            $"'{TypeNameHelper.GetDisplayName(implementationType)}' has several constructors marked with [InjectionConstructor]. " +
            "Mark exactly one of them.");
    }

    public static ResolutionException NoSatisfiableConstructor(Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(implementationType);

        return new ResolutionException(
            $"None of the public constructors of '{TypeNameHelper.GetDisplayName(implementationType)}' can be satisfied: " +
            "each of them requires at least one service that is not registered. Register the missing services, " +
            "or mark the constructor to use with [InjectionConstructor] to get a precise diagnostic.");
    }

    public static ResolutionException AmbiguousConstructors(Type implementationType, int parameterCount)
    {
        ArgumentNullException.ThrowIfNull(implementationType);

        return new ResolutionException(
            $"'{TypeNameHelper.GetDisplayName(implementationType)}' has several satisfiable constructors with {parameterCount} parameter(s), " +
            "so the choice would be arbitrary. Mark the constructor to use with [InjectionConstructor].");
    }

    public static ResolutionException CannotCloseGeneric(Type openImplementation, Type closedService, Exception? inner = null)
    {
        ArgumentNullException.ThrowIfNull(openImplementation);
        ArgumentNullException.ThrowIfNull(closedService);

        return new ResolutionException(
            $"The open generic implementation '{TypeNameHelper.GetDisplayName(openImplementation)}' cannot be closed for " +
            $"'{TypeNameHelper.GetDisplayName(closedService)}': its generic arguments violate the implementation's constraints, " +
            "or the implementation does not declare its type parameters in the same order as the service.",
            inner);
    }
}
