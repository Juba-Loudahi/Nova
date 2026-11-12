namespace Nova.DI.Internal;

internal sealed class ServiceProviderCallSite : ServiceCallSite
{
    public ServiceProviderCallSite()
        : base(ResultCache.None(typeof(IServiceProvider)))
    {
    }

    public override Type ServiceType => typeof(IServiceProvider);

    public override Type? ImplementationType => null;

    public override CallSiteKind Kind => CallSiteKind.ServiceProvider;
}
