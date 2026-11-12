using System.Runtime.CompilerServices;

namespace Nova.DI.Internal;

internal abstract class CallSiteVisitor<TArgument, TResult>
{
    protected virtual TResult VisitCallSite(ServiceCallSite callSite, TArgument argument)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();

        return callSite.Cache.Location switch
        {
            CacheLocation.Root => VisitRootCache(callSite, argument),
            CacheLocation.Scope => VisitScopeCache(callSite, argument),
            CacheLocation.Dispose => VisitDisposeCache(callSite, argument),
            CacheLocation.None => VisitNoCache(callSite, argument),
            _ => throw new NotSupportedException($"Cache location '{callSite.Cache.Location}' is not supported."),
        };
    }

    protected virtual TResult VisitRootCache(ServiceCallSite callSite, TArgument argument) => VisitCallSiteMain(callSite, argument);

    protected virtual TResult VisitScopeCache(ServiceCallSite callSite, TArgument argument) => VisitCallSiteMain(callSite, argument);

    protected virtual TResult VisitDisposeCache(ServiceCallSite callSite, TArgument argument) => VisitCallSiteMain(callSite, argument);

    protected virtual TResult VisitNoCache(ServiceCallSite callSite, TArgument argument) => VisitCallSiteMain(callSite, argument);

    protected virtual TResult VisitCallSiteMain(ServiceCallSite callSite, TArgument argument) => callSite.Kind switch
    {
        CallSiteKind.Constructor => VisitConstructor((ConstructorCallSite)callSite, argument),
        CallSiteKind.Factory => VisitFactory((FactoryCallSite)callSite, argument),
        CallSiteKind.Constant => VisitConstant((ConstantCallSite)callSite, argument),
        CallSiteKind.Enumerable => VisitEnumerable((EnumerableCallSite)callSite, argument),
        CallSiteKind.ServiceProvider => VisitServiceProvider((ServiceProviderCallSite)callSite, argument),
        CallSiteKind.ScopeFactory => VisitScopeFactory((ScopeFactoryCallSite)callSite, argument),
        _ => throw new NotSupportedException($"Call site kind '{callSite.Kind}' is not supported."),
    };

    protected abstract TResult VisitConstructor(ConstructorCallSite callSite, TArgument argument);

    protected abstract TResult VisitFactory(FactoryCallSite callSite, TArgument argument);

    protected abstract TResult VisitConstant(ConstantCallSite callSite, TArgument argument);

    protected abstract TResult VisitEnumerable(EnumerableCallSite callSite, TArgument argument);

    protected abstract TResult VisitServiceProvider(ServiceProviderCallSite callSite, TArgument argument);

    protected abstract TResult VisitScopeFactory(ScopeFactoryCallSite callSite, TArgument argument);
}
