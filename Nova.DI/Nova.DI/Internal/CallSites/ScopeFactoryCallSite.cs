namespace Nova.DI.Internal;

internal sealed class ScopeFactoryCallSite : ServiceCallSite
{
    public ScopeFactoryCallSite()
        : base(ResultCache.None(typeof(IServiceScopeFactory)))
    {
    }

    public override Type ServiceType => typeof(IServiceScopeFactory);

    public override Type? ImplementationType => null;

    public override CallSiteKind Kind => CallSiteKind.ScopeFactory;
}
