namespace Nova.DI.Internal;

internal enum CallSiteKind
{
    Constructor,
    Factory,
    Constant,
    Enumerable,
    ServiceProvider,
    ScopeFactory,
}
