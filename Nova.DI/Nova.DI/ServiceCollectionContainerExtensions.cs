namespace Nova.DI;

public static class ServiceCollectionContainerExtensions
{
    public static ServiceProvider BuildServiceProvider(this IServiceCollection services, ServiceProviderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        ServiceDescriptor[] descriptors = [.. services];
        if (services is ServiceCollection collection)
        {
            collection.MakeReadOnly();
        }

        return new ServiceProvider(descriptors, options ?? new ServiceProviderOptions());
    }
}
