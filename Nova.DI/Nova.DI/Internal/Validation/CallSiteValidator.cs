using System.Collections.Concurrent;

namespace Nova.DI.Internal;

internal sealed class CallSiteValidator : CallSiteVisitor<CallSiteValidator.State, Type?>
{
    private readonly ConcurrentDictionary<ServiceCallSite, Type?> _scopedDependencies = new();

    public Type? Validate(ServiceCallSite callSite)
        => _scopedDependencies.GetOrAdd(
            callSite,
            static (site, validator) => validator.VisitCallSite(site, default),
            this);

    protected override Type? VisitRootCache(ServiceCallSite callSite, State state)
        => VisitCallSiteMain(callSite, new State(callSite));

    protected override Type? VisitScopeCache(ServiceCallSite callSite, State state)
    {
        if (state.Singleton is { } singleton)
        {
            throw new CaptiveDependencyException(singleton.ServiceType, callSite.ServiceType);
        }

        VisitCallSiteMain(callSite, state);
        return callSite.ServiceType;
    }

    protected override Type? VisitConstructor(ConstructorCallSite callSite, State state)
        => VisitChildren(callSite.ParameterCallSites, state);

    protected override Type? VisitEnumerable(EnumerableCallSite callSite, State state)
        => VisitChildren(callSite.Items, state);

    protected override Type? VisitFactory(FactoryCallSite callSite, State state) => null;

    protected override Type? VisitConstant(ConstantCallSite callSite, State state) => null;

    protected override Type? VisitServiceProvider(ServiceProviderCallSite callSite, State state) => null;

    protected override Type? VisitScopeFactory(ScopeFactoryCallSite callSite, State state) => null;

    private Type? VisitChildren(ServiceCallSite[] children, State state)
    {
        Type? firstScoped = null;

        foreach (ServiceCallSite child in children)
        {
            Type? scoped = VisitCallSite(child, state);
            firstScoped ??= scoped;
        }

        return firstScoped;
    }

    internal readonly record struct State(ServiceCallSite? Singleton);
}
