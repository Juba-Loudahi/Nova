using System.Collections.Concurrent;
using Nova.DI.Internal;

namespace Nova.DI;

public sealed class ServiceProvider : IServiceProvider, IServiceScopeFactory, IDisposable, IAsyncDisposable
{
    private readonly CallSiteFactory _callSiteFactory;
    private readonly CallSiteValidator? _validator;
    private readonly ConcurrentDictionary<ServiceIdentifier, ServiceAccessor> _accessors = new();
    private readonly Func<ServiceIdentifier, ServiceAccessor> _createAccessor;

    internal ServiceProvider(ServiceDescriptor[] descriptors, ServiceProviderOptions options)
    {
        RootScope = new ServiceScope(this, isRoot: true);
        _callSiteFactory = new CallSiteFactory(descriptors);
        _validator = options.ValidateScopes ? new CallSiteValidator() : null;
        _createAccessor = CreateAccessor;

        if (options.ValidateOnBuild)
        {
            ValidateAll(descriptors.Length);
        }
    }

    internal ServiceScope RootScope { get; }

    public object? GetService(Type serviceType) => GetService(serviceType, RootScope);

    public IServiceScope CreateScope()
    {
        ObjectDisposedException.ThrowIf(RootScope.IsDisposed, this);
        return new ServiceScope(this, isRoot: false);
    }

    public bool IsService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return _callSiteFactory.IsService(serviceType);
    }

    public void Dispose() => RootScope.Dispose();

    public ValueTask DisposeAsync() => RootScope.DisposeAsync();

    internal object? GetService(Type serviceType, ServiceScope scope)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ObjectDisposedException.ThrowIf(RootScope.IsDisposed, this);

        if (serviceType.ContainsGenericParameters)
        {
            throw new ArgumentException(
                $"'{TypeNameHelper.GetDisplayName(serviceType)}' is an open generic type and cannot be resolved: request a closed type instead.",
                nameof(serviceType));
        }

        ServiceAccessor accessor = _accessors.GetOrAdd(new ServiceIdentifier(serviceType), _createAccessor);
        ServiceCallSite? callSite = accessor.CallSite;
        if (callSite is null)
        {
            return null;
        }

        if (scope.IsRoot && accessor.ScopedDependency is { } scopedDependency)
        {
            throw ScopeValidationException.ResolvedFromRoot(serviceType, scopedDependency);
        }

        return RuntimeResolver.Instance.Resolve(callSite, scope);
    }

    private ServiceAccessor CreateAccessor(ServiceIdentifier id)
    {
        ServiceCallSite? callSite = _callSiteFactory.GetCallSite(id, new CallSiteChain());
        if (callSite is null)
        {
            return ServiceAccessor.NotFound;
        }

        return new ServiceAccessor(callSite, _validator?.Validate(callSite));
    }

    private void ValidateAll(int descriptorCount)
    {
        List<Exception>? errors = null;

        for (int index = 0; index < descriptorCount; index++)
        {
            try
            {
                ServiceCallSite? callSite = _callSiteFactory.GetCallSite(index, new CallSiteChain());
                if (callSite is not null)
                {
                    _validator?.Validate(callSite);
                }
            }
            catch (DependencyInjectionException exception)
            {
                (errors ??= []).Add(exception);
            }
        }

        if (errors is not null)
        {
            throw new AggregateException(
                $"The container is misconfigured: {errors.Count} registration(s) can never be constructed.",
                errors);
        }
    }
}
