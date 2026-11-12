namespace Nova.DI.Internal;

internal readonly struct ResultCache
{
    public ResultCache(CacheLocation location, ServiceCacheKey key)
    {
        Location = location;
        Key = key;
    }

    public CacheLocation Location { get; }

    public ServiceCacheKey Key { get; }

    public static ResultCache None(Type serviceType)
        => new(CacheLocation.None, new ServiceCacheKey(new ServiceIdentifier(serviceType), ServiceCacheKey.NoDescriptor));

    public static ResultCache For(ServiceLifetime lifetime, ServiceCacheKey key, Type? implementationType) => lifetime switch
    {
        ServiceLifetime.Singleton => new ResultCache(CacheLocation.Root, key),
        ServiceLifetime.Scoped => new ResultCache(CacheLocation.Scope, key),
        ServiceLifetime.Transient => new ResultCache(
            implementationType is null || IsDisposable(implementationType) ? CacheLocation.Dispose : CacheLocation.None,
            key),
        _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Unknown service lifetime."),
    };

    private static bool IsDisposable(Type type)
        => typeof(IDisposable).IsAssignableFrom(type) || typeof(IAsyncDisposable).IsAssignableFrom(type);
}
