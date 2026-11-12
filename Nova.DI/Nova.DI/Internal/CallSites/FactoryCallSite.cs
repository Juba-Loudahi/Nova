namespace Nova.DI.Internal;

internal sealed class FactoryCallSite : ServiceCallSite
{
    public FactoryCallSite(ResultCache cache, Type serviceType, Func<IServiceProvider, object> factory)
        : base(cache)
    {
        ServiceType = serviceType;
        Factory = factory;
    }

    public override Type ServiceType { get; }

    public override Type? ImplementationType => null;

    public override CallSiteKind Kind => CallSiteKind.Factory;

    public Func<IServiceProvider, object> Factory { get; }
}
