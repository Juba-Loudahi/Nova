using Nova.DI.Internal;

namespace Nova.DI;

public sealed class ServiceDescriptor
{
    private ServiceDescriptor(
        Type serviceType,
        ServiceLifetime lifetime,
        Type? implementationType,
        object? implementationInstance,
        Func<IServiceProvider, object>? implementationFactory)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
        ImplementationType = implementationType;
        ImplementationInstance = implementationInstance;
        ImplementationFactory = implementationFactory;
    }

    public Type ServiceType { get; }

    public ServiceLifetime Lifetime { get; }

    public Type? ImplementationType { get; }

    public object? ImplementationInstance { get; }

    public Func<IServiceProvider, object>? ImplementationFactory { get; }

    public static ServiceDescriptor Describe(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);
        EnsureDefined(lifetime);
        TypeGuard.EnsureValidImplementation(serviceType, implementationType);

        return new ServiceDescriptor(serviceType, lifetime, implementationType, null, null);
    }

    public static ServiceDescriptor Instance(Type serviceType, object instance)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(instance);

        if (!serviceType.IsInstanceOfType(instance))
        {
            throw new ArgumentException(
                $"The instance of type '{TypeNameHelper.GetDisplayName(instance.GetType())}' is not assignable to '{TypeNameHelper.GetDisplayName(serviceType)}'.",
                nameof(instance));
        }

        return new ServiceDescriptor(serviceType, ServiceLifetime.Singleton, null, instance, null);
    }

    public static ServiceDescriptor Factory(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(factory);
        EnsureDefined(lifetime);

        if (serviceType.ContainsGenericParameters)
        {
            throw new ArgumentException(
                $"The open generic service '{TypeNameHelper.GetDisplayName(serviceType)}' cannot be registered with a factory: register an open generic implementation type instead.",
                nameof(serviceType));
        }

        return new ServiceDescriptor(serviceType, lifetime, null, null, factory);
    }

    public override string ToString()
    {
        string service = TypeNameHelper.GetDisplayName(ServiceType);

        if (ImplementationType is not null)
        {
            return $"{service} -> type {TypeNameHelper.GetDisplayName(ImplementationType)} ({Lifetime})";
        }

        return ImplementationInstance is not null
            ? $"{service} -> instance ({Lifetime})"
            : $"{service} -> factory ({Lifetime})";
    }

    private static void EnsureDefined(ServiceLifetime lifetime)
    {
        if (!Enum.IsDefined(lifetime))
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Unknown service lifetime.");
        }
    }
}
