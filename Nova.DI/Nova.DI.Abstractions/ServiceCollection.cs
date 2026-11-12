using System.Collections;

namespace Nova.DI;

public sealed class ServiceCollection : IServiceCollection
{
    private readonly List<ServiceDescriptor> _descriptors = [];

    public int Count => _descriptors.Count;

    public bool IsReadOnly { get; private set; }

    public ServiceDescriptor this[int index]
    {
        get => _descriptors[index];
        set
        {
            ThrowIfReadOnly();
            ArgumentNullException.ThrowIfNull(value);
            _descriptors[index] = value;
        }
    }

    public void Add(ServiceDescriptor item)
    {
        ThrowIfReadOnly();
        ArgumentNullException.ThrowIfNull(item);
        _descriptors.Add(item);
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        ThrowIfReadOnly();
        ArgumentNullException.ThrowIfNull(item);
        _descriptors.Insert(index, item);
    }

    public bool Remove(ServiceDescriptor item)
    {
        ThrowIfReadOnly();
        return _descriptors.Remove(item);
    }

    public void RemoveAt(int index)
    {
        ThrowIfReadOnly();
        _descriptors.RemoveAt(index);
    }

    public void Clear()
    {
        ThrowIfReadOnly();
        _descriptors.Clear();
    }

    public int IndexOf(ServiceDescriptor item) => _descriptors.IndexOf(item);

    public bool Contains(ServiceDescriptor item) => _descriptors.Contains(item);

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _descriptors.CopyTo(array, arrayIndex);

    public IEnumerator<ServiceDescriptor> GetEnumerator() => _descriptors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void MakeReadOnly() => IsReadOnly = true;

    private void ThrowIfReadOnly()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException(
                "The service collection is read-only: the container has already been built. Register every service before calling BuildServiceProvider().");
        }
    }
}
