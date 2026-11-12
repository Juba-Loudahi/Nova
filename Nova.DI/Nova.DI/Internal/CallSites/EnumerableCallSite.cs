namespace Nova.DI.Internal;

internal sealed class EnumerableCallSite : ServiceCallSite
{
    public EnumerableCallSite(Type itemType, ServiceCallSite[] items)
        : this(typeof(IEnumerable<>).MakeGenericType(itemType), itemType, items)
    {
    }

    private EnumerableCallSite(Type serviceType, Type itemType, ServiceCallSite[] items)
        : base(ResultCache.None(serviceType))
    {
        ServiceType = serviceType;
        ItemType = itemType;
        Items = items;
    }

    public override Type ServiceType { get; }

    public override Type? ImplementationType => ItemType.MakeArrayType();

    public override CallSiteKind Kind => CallSiteKind.Enumerable;

    public Type ItemType { get; }

    public ServiceCallSite[] Items { get; }
}
