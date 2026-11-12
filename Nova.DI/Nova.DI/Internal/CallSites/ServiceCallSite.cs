namespace Nova.DI.Internal;

internal abstract class ServiceCallSite
{
    private object? _value;

    protected ServiceCallSite(ResultCache cache)
    {
        Cache = cache;
    }

    public abstract Type ServiceType { get; }

    public abstract Type? ImplementationType { get; }

    public abstract CallSiteKind Kind { get; }

    public ResultCache Cache { get; }

    public object? Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }
}
