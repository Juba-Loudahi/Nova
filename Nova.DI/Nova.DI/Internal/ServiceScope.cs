using System.Runtime.ExceptionServices;

namespace Nova.DI.Internal;

internal sealed class ServiceScope : IServiceScope, IServiceProvider
{
    private static readonly object InProgress = new();

    private readonly Dictionary<ServiceCacheKey, object?> _resolved = [];
    private readonly Lock _sync = new();
    private List<object>? _disposables;
    private List<ServiceCacheKey>? _creating;
    private bool _disposed;

    public ServiceScope(ServiceProvider rootProvider, bool isRoot)
    {
        RootProvider = rootProvider;
        IsRoot = isRoot;
    }

    public ServiceProvider RootProvider { get; }

    public bool IsRoot { get; }

    public bool IsDisposed => Volatile.Read(ref _disposed);

    public IServiceProvider ServiceProvider => this;

    public object? GetService(Type serviceType)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        return RootProvider.GetService(serviceType, this);
    }

    internal object? GetOrCreate<TState>(ServiceCacheKey key, TState state, Func<TState, ServiceScope, object?> create)
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_resolved.TryGetValue(key, out object? existing))
            {
                if (ReferenceEquals(existing, InProgress))
                {
                    throw new CircularDependencyException(DescribeCycle(key));
                }

                return existing;
            }

            List<ServiceCacheKey> creating = _creating ??= [];
            _resolved[key] = InProgress;
            creating.Add(key);

            try
            {
                object? created = create(state, this);
                _resolved[key] = created;
                CaptureDisposable(created);
                return created;
            }
            catch
            {
                _resolved.Remove(key);
                throw;
            }
            finally
            {
                creating.RemoveAt(creating.Count - 1);
            }
        }
    }

    internal object? CaptureDisposable(object? service)
    {
        if (service is null || ReferenceEquals(service, this) || (service is not IDisposable && service is not IAsyncDisposable))
        {
            return service;
        }

        bool disposed;
        lock (_sync)
        {
            disposed = _disposed;
            if (!disposed)
            {
                (_disposables ??= []).Add(service);
            }
        }

        if (disposed)
        {
            DisposeOrphan(service);
            throw new ObjectDisposedException(nameof(ServiceScope));
        }

        return service;
    }

    public void Dispose()
    {
        List<object>? disposables = BeginDispose();
        if (disposables is null)
        {
            return;
        }

        List<Exception>? errors = null;
        for (int index = disposables.Count - 1; index >= 0; index--)
        {
            object service = disposables[index];
            if (service is not IDisposable disposable)
            {
                (errors ??= []).Add(new InvalidOperationException(
                    $"'{TypeNameHelper.GetDisplayName(service.GetType())}' only implements IAsyncDisposable and cannot be disposed synchronously. " +
                    "Dispose the scope with DisposeAsync() (await using) instead."));
                continue;
            }

            try
            {
                disposable.Dispose();
            }
            catch (Exception exception)
            {
                (errors ??= []).Add(exception);
            }
        }

        ThrowIfAny(errors);
    }

    public async ValueTask DisposeAsync()
    {
        List<object>? disposables = BeginDispose();
        if (disposables is null)
        {
            return;
        }

        List<Exception>? errors = null;
        for (int index = disposables.Count - 1; index >= 0; index--)
        {
            try
            {
                if (disposables[index] is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    ((IDisposable)disposables[index]).Dispose();
                }
            }
            catch (Exception exception)
            {
                (errors ??= []).Add(exception);
            }
        }

        ThrowIfAny(errors);
    }

    private List<object>? BeginDispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return null;
            }

            Volatile.Write(ref _disposed, true);
            _resolved.Clear();

            List<object>? disposables = _disposables;
            _disposables = null;
            return disposables;
        }
    }

    private List<Type> DescribeCycle(ServiceCacheKey repeated)
    {
        List<Type> path = [];
        if (_creating is not null)
        {
            int start = Math.Max(_creating.IndexOf(repeated), 0);
            for (int index = start; index < _creating.Count; index++)
            {
                path.Add(_creating[index].Identifier.ServiceType);
            }
        }

        path.Add(repeated.Identifier.ServiceType);
        return path;
    }

    private static void ThrowIfAny(List<Exception>? errors)
    {
        if (errors is null)
        {
            return;
        }

        if (errors.Count == 1)
        {
            ExceptionDispatchInfo.Throw(errors[0]);
        }

        throw new AggregateException("Several services failed to dispose.", errors);
    }

    private static void DisposeOrphan(object service)
    {
        if (service is IDisposable disposable)
        {
            disposable.Dispose();
        }
        else if (service is IAsyncDisposable asyncDisposable)
        {
            Task.Run(() => asyncDisposable.DisposeAsync().AsTask()).GetAwaiter().GetResult();
        }
    }
}
