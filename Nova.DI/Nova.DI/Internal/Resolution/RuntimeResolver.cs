using System.Reflection;

namespace Nova.DI.Internal;

internal sealed class RuntimeResolver : CallSiteVisitor<ServiceScope, object?>
{
    private RuntimeResolver()
    {
    }

    public static RuntimeResolver Instance { get; } = new();

    public object? Resolve(ServiceCallSite callSite, ServiceScope scope) => VisitCallSite(callSite, scope);

    protected override object? VisitRootCache(ServiceCallSite callSite, ServiceScope scope)
    {
        object? value = callSite.Value;
        if (value is not null)
        {
            return value;
        }

        value = scope.RootProvider.RootScope.GetOrCreate(
            callSite.Cache.Key,
            callSite,
            static (site, root) => Instance.VisitCallSiteMain(site, root));

        callSite.Value = value;
        return value;
    }

    protected override object? VisitScopeCache(ServiceCallSite callSite, ServiceScope scope)
        => scope.GetOrCreate(
            callSite.Cache.Key,
            callSite,
            static (site, current) => Instance.VisitCallSiteMain(site, current));

    protected override object? VisitDisposeCache(ServiceCallSite callSite, ServiceScope scope)
        => scope.CaptureDisposable(VisitCallSiteMain(callSite, scope));

    protected override object? VisitConstructor(ConstructorCallSite callSite, ServiceScope scope)
    {
        ServiceCallSite[] parameterCallSites = callSite.ParameterCallSites;
        object?[] arguments = parameterCallSites.Length == 0 ? [] : new object?[parameterCallSites.Length];

        for (int index = 0; index < parameterCallSites.Length; index++)
        {
            arguments[index] = VisitCallSite(parameterCallSites[index], scope);
        }

        return callSite.Constructor.Invoke(BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);
    }

    protected override object? VisitFactory(FactoryCallSite callSite, ServiceScope scope) => callSite.Factory(scope);

    protected override object? VisitConstant(ConstantCallSite callSite, ServiceScope scope) => callSite.Instance;

    protected override object? VisitEnumerable(EnumerableCallSite callSite, ServiceScope scope)
    {
        ServiceCallSite[] items = callSite.Items;
        Array array = Array.CreateInstance(callSite.ItemType, items.Length);

        for (int index = 0; index < items.Length; index++)
        {
            array.SetValue(VisitCallSite(items[index], scope), index);
        }

        return array;
    }

    protected override object? VisitServiceProvider(ServiceProviderCallSite callSite, ServiceScope scope) => scope;

    protected override object? VisitScopeFactory(ScopeFactoryCallSite callSite, ServiceScope scope) => scope.RootProvider;
}
