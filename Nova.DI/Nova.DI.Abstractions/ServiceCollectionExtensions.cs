namespace Nova.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => AddType(services, typeof(TService), typeof(TImplementation), ServiceLifetime.Transient);

    public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
        where TService : class
        => AddType(services, typeof(TService), typeof(TService), ServiceLifetime.Transient);

    public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationType)
        => AddType(services, serviceType, implementationType, ServiceLifetime.Transient);

    public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
        where TService : class
        => AddFactory(services, typeof(TService), factory, ServiceLifetime.Transient);

    public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => AddType(services, typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped);

    public static IServiceCollection AddScoped<TService>(this IServiceCollection services)
        where TService : class
        => AddType(services, typeof(TService), typeof(TService), ServiceLifetime.Scoped);

    public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Type implementationType)
        => AddType(services, serviceType, implementationType, ServiceLifetime.Scoped);

    public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
        where TService : class
        => AddFactory(services, typeof(TService), factory, ServiceLifetime.Scoped);

    public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => AddType(services, typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton);

    public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
        where TService : class
        => AddType(services, typeof(TService), typeof(TService), ServiceLifetime.Singleton);

    public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Type implementationType)
        => AddType(services, serviceType, implementationType, ServiceLifetime.Singleton);

    public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
        where TService : class
        => AddFactory(services, typeof(TService), factory, ServiceLifetime.Singleton);

    public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService instance)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        return AddDescriptor(services, ServiceDescriptor.Instance(typeof(TService), instance));
    }

    public static IServiceCollection TryAdd(this IServiceCollection services, ServiceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(descriptor);

        foreach (ServiceDescriptor existing in services)
        {
            if (existing.ServiceType == descriptor.ServiceType)
            {
                return services;
            }
        }

        return AddDescriptor(services, descriptor);
    }

    public static IServiceCollection TryAddTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => TryAdd(services, ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient));

    public static IServiceCollection TryAddScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => TryAdd(services, ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped));

    public static IServiceCollection TryAddSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => TryAdd(services, ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton));

    private static IServiceCollection AddType(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(services);
        return AddDescriptor(services, ServiceDescriptor.Describe(serviceType, implementationType, lifetime));
    }

    private static IServiceCollection AddFactory(IServiceCollection services, Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(services);
        return AddDescriptor(services, ServiceDescriptor.Factory(serviceType, factory, lifetime));
    }

    private static IServiceCollection AddDescriptor(this IServiceCollection services, ServiceDescriptor descriptor)
    {
        services.Add(descriptor);
        return services;
    }
}
