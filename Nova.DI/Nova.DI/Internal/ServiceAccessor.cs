namespace Nova.DI.Internal;

internal sealed class ServiceAccessor
{
    public static readonly ServiceAccessor NotFound = new(null, null);

    public ServiceAccessor(ServiceCallSite? callSite, Type? scopedDependency)
    {
        CallSite = callSite;
        ScopedDependency = scopedDependency;
    }

    public ServiceCallSite? CallSite { get; }

    public Type? ScopedDependency { get; }
}
