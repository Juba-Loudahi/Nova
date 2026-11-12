namespace Nova.DI.Internal;

internal readonly struct ServiceIdentifier : IEquatable<ServiceIdentifier>
{
    public ServiceIdentifier(Type serviceType)
    {
        ServiceType = serviceType;
    }

    public Type ServiceType { get; }

    public static bool operator ==(ServiceIdentifier left, ServiceIdentifier right) => left.Equals(right);

    public static bool operator !=(ServiceIdentifier left, ServiceIdentifier right) => !left.Equals(right);

    public bool Equals(ServiceIdentifier other) => ServiceType == other.ServiceType;

    public override bool Equals(object? obj) => obj is ServiceIdentifier other && Equals(other);

    public override int GetHashCode() => ServiceType.GetHashCode();

    public override string ToString() => TypeNameHelper.GetDisplayName(ServiceType);
}
