using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;

namespace Nova.DI.Internal;

internal sealed class CallSiteFactory
{
    private readonly ServiceDescriptor[] _descriptors;
    private readonly FrozenDictionary<Type, int[]> _indices;
    private readonly FrozenDictionary<Type, ServiceCallSite> _builtIns;
    private readonly ConcurrentDictionary<ServiceCacheKey, ServiceCallSite> _cache = new();
    private readonly ConstructorSelector _constructorSelector;

    public CallSiteFactory(ServiceDescriptor[] descriptors)
    {
        _descriptors = descriptors;

        Dictionary<Type, List<int>> indices = [];
        for (int index = 0; index < descriptors.Length; index++)
        {
            Type serviceType = descriptors[index].ServiceType;
            if (!indices.TryGetValue(serviceType, out List<int>? list))
            {
                list = [];
                indices.Add(serviceType, list);
            }

            list.Add(index);
        }

        _indices = indices.ToFrozenDictionary(static entry => entry.Key, static entry => entry.Value.ToArray());
        _builtIns = new Dictionary<Type, ServiceCallSite>
        {
            [typeof(IServiceProvider)] = new ServiceProviderCallSite(),
            [typeof(IServiceScopeFactory)] = new ScopeFactoryCallSite(),
        }.ToFrozenDictionary();
        _constructorSelector = new ConstructorSelector(IsService);
    }

    public bool IsService(Type type)
    {
        if (type.ContainsGenericParameters)
        {
            return false;
        }

        if (_builtIns.ContainsKey(type) || _indices.ContainsKey(type))
        {
            return true;
        }

        if (type.IsConstructedGenericType)
        {
            Type definition = type.GetGenericTypeDefinition();
            return definition == typeof(IEnumerable<>) || _indices.ContainsKey(definition);
        }

        return false;
    }

    public ServiceCallSite? GetCallSite(ServiceIdentifier id, CallSiteChain chain)
    {
        Type serviceType = id.ServiceType;

        if (_builtIns.TryGetValue(serviceType, out ServiceCallSite? builtIn))
        {
            return builtIn;
        }

        if (_indices.TryGetValue(serviceType, out int[]? exact))
        {
            return CreateForDescriptor(exact[^1], id, chain, throwOnConstraintViolation: true);
        }

        if (serviceType.IsConstructedGenericType)
        {
            Type definition = serviceType.GetGenericTypeDefinition();

            if (definition == typeof(IEnumerable<>))
            {
                return CreateEnumerable(id, chain);
            }

            if (_indices.TryGetValue(definition, out int[]? open))
            {
                return CreateForDescriptor(open[^1], id, chain, throwOnConstraintViolation: true);
            }
        }

        return null;
    }

    public ServiceCallSite? GetCallSite(int descriptorIndex, CallSiteChain chain)
    {
        Type serviceType = _descriptors[descriptorIndex].ServiceType;
        if (serviceType.IsGenericTypeDefinition)
        {
            return null;
        }

        return CreateForDescriptor(descriptorIndex, new ServiceIdentifier(serviceType), chain, throwOnConstraintViolation: true);
    }

    private ServiceCallSite? CreateForDescriptor(int index, ServiceIdentifier id, CallSiteChain chain, bool throwOnConstraintViolation)
    {
        ServiceCacheKey key = new(id, index);
        if (_cache.TryGetValue(key, out ServiceCallSite? cached))
        {
            return cached;
        }

        ServiceDescriptor descriptor = _descriptors[index];
        ServiceCallSite callSite;

        if (descriptor.ImplementationInstance is { } instance)
        {
            callSite = new ConstantCallSite(id.ServiceType, instance);
        }
        else if (descriptor.ImplementationFactory is { } factory)
        {
            callSite = new FactoryCallSite(ResultCache.For(descriptor.Lifetime, key, null), id.ServiceType, factory);
        }
        else
        {
            Type implementation = descriptor.ImplementationType!;
            if (implementation.IsGenericTypeDefinition)
            {
                Type? closed = CloseGeneric(implementation, id.ServiceType, throwOnConstraintViolation);
                if (closed is null)
                {
                    return null;
                }

                implementation = closed;
            }

            callSite = CreateConstructorCallSite(ResultCache.For(descriptor.Lifetime, key, implementation), key, implementation, chain);
        }

        return _cache.GetOrAdd(key, callSite);
    }

    private ConstructorCallSite CreateConstructorCallSite(ResultCache cache, ServiceCacheKey key, Type implementation, CallSiteChain chain)
    {
        chain.Enter(key, implementation);
        try
        {
            ConstructorInfo constructor = _constructorSelector.Select(implementation);
            ParameterInfo[] parameters = constructor.GetParameters();
            ServiceCallSite[] parameterCallSites = parameters.Length == 0 ? [] : new ServiceCallSite[parameters.Length];

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];
                ServiceCallSite? parameterCallSite = GetCallSite(new ServiceIdentifier(parameter.ParameterType), chain);

                if (parameterCallSite is null)
                {
                    if (!ParameterDefaultValue.TryGet(parameter, out object? defaultValue))
                    {
                        throw ResolutionException.MissingDependency(parameter.ParameterType, implementation, chain.DescribePath());
                    }

                    parameterCallSite = new ConstantCallSite(parameter.ParameterType, defaultValue);
                }

                parameterCallSites[index] = parameterCallSite;
            }

            return new ConstructorCallSite(cache, key.Identifier.ServiceType, constructor, parameterCallSites);
        }
        finally
        {
            chain.Exit(key);
        }
    }

    private ServiceCallSite CreateEnumerable(ServiceIdentifier id, CallSiteChain chain)
    {
        ServiceCacheKey key = new(id, ServiceCacheKey.NoDescriptor);
        if (_cache.TryGetValue(key, out ServiceCallSite? cached))
        {
            return cached;
        }

        Type itemType = id.ServiceType.GenericTypeArguments[0];
        ServiceIdentifier itemId = new(itemType);
        List<ServiceCallSite> items = [];

        chain.Enter(key, null);
        try
        {
            foreach (int index in MatchingDescriptors(itemType))
            {
                ServiceCallSite? item = CreateForDescriptor(index, itemId, chain, throwOnConstraintViolation: false);
                if (item is not null)
                {
                    items.Add(item);
                }
            }
        }
        finally
        {
            chain.Exit(key);
        }

        return _cache.GetOrAdd(key, new EnumerableCallSite(itemType, [.. items]));
    }

    private IEnumerable<int> MatchingDescriptors(Type itemType)
    {
        _indices.TryGetValue(itemType, out int[]? exact);

        int[]? open = null;
        if (itemType.IsConstructedGenericType)
        {
            _indices.TryGetValue(itemType.GetGenericTypeDefinition(), out open);
        }

        if (open is null)
        {
            return exact ?? [];
        }

        if (exact is null)
        {
            return open;
        }

        return exact.Concat(open).Order();
    }

    private static Type? CloseGeneric(Type openImplementation, Type closedService, bool throwOnConstraintViolation)
    {
        Type closed;
        try
        {
            closed = openImplementation.MakeGenericType(closedService.GenericTypeArguments);
        }
        catch (ArgumentException) when (!throwOnConstraintViolation)
        {
            return null;
        }
        catch (ArgumentException exception)
        {
            throw ResolutionException.CannotCloseGeneric(openImplementation, closedService, exception);
        }

        if (closedService.IsAssignableFrom(closed))
        {
            return closed;
        }

        return throwOnConstraintViolation
            ? throw ResolutionException.CannotCloseGeneric(openImplementation, closedService)
            : null;
    }
}
