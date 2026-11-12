namespace Nova.DI.Internal;

internal sealed class ConstantCallSite : ServiceCallSite
{
    public ConstantCallSite(Type serviceType, object? instance)
        : base(ResultCache.None(serviceType))
    {
        ServiceType = serviceType;
        Instance = instance;
    }

    public override Type ServiceType { get; }

    public override Type? ImplementationType => Instance?.GetType();

    public override CallSiteKind Kind => CallSiteKind.Constant;

    public object? Instance { get; }
}
