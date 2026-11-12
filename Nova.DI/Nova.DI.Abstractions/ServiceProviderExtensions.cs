namespace Nova.DI;

public static class ServiceProviderExtensions
{
    public static T? GetService<T>(this IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        return (T?)provider.GetService(typeof(T));
    }

    public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(serviceType);

        return provider.GetService(serviceType) ?? throw ResolutionException.NotRegistered(serviceType);
    }

    public static T GetRequiredService<T>(this IServiceProvider provider)
        where T : notnull
        => (T)provider.GetRequiredService(typeof(T));

    public static IEnumerable<T> GetServices<T>(this IServiceProvider provider)
        => provider.GetRequiredService<IEnumerable<T>>();

    public static IServiceScope CreateScope(this IServiceProvider provider)
        => provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
}
