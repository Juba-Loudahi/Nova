using System.Reflection;

namespace Nova.DI.Internal;

internal sealed class ConstructorCallSite : ServiceCallSite
{
    public ConstructorCallSite(ResultCache cache, Type serviceType, ConstructorInfo constructor, ServiceCallSite[] parameterCallSites)
        : base(cache)
    {
        ServiceType = serviceType;
        Constructor = constructor;
        ParameterCallSites = parameterCallSites;
    }

    public override Type ServiceType { get; }

    public override Type? ImplementationType => Constructor.DeclaringType;

    public override CallSiteKind Kind => CallSiteKind.Constructor;

    public ConstructorInfo Constructor { get; }

    public ServiceCallSite[] ParameterCallSites { get; }
}
