namespace Nova.DI.Internal;

internal static class TypeGuard
{
    public static void EnsureValidImplementation(Type serviceType, Type implementationType)
    {
        string implementationName = TypeNameHelper.GetDisplayName(implementationType);
        string serviceName = TypeNameHelper.GetDisplayName(serviceType);

        if (implementationType.IsAbstract || implementationType.IsInterface)
        {
            throw new ArgumentException(
                $"'{implementationName}' cannot be used as an implementation: it is abstract or an interface and cannot be instantiated.",
                nameof(implementationType));
        }

        if (serviceType.IsGenericTypeDefinition)
        {
            if (!implementationType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    $"The open generic service '{serviceName}' requires an open generic implementation, but '{implementationName}' is not.",
                    nameof(implementationType));
            }

            if (serviceType.GetGenericArguments().Length != implementationType.GetGenericArguments().Length)
            {
                throw new ArgumentException(
                    $"'{implementationName}' and '{serviceName}' do not have the same number of generic type parameters.",
                    nameof(implementationType));
            }

            if (!ImplementsOpenGeneric(implementationType, serviceType))
            {
                throw new ArgumentException(
                    $"'{implementationName}' does not implement '{serviceName}'.",
                    nameof(implementationType));
            }

            return;
        }

        if (serviceType.ContainsGenericParameters)
        {
            throw new ArgumentException(
                $"'{serviceName}' contains unresolved generic parameters and is not a generic type definition.",
                nameof(serviceType));
        }

        if (implementationType.ContainsGenericParameters)
        {
            throw new ArgumentException(
                $"'{implementationName}' contains unresolved generic parameters, but the service '{serviceName}' is not an open generic type.",
                nameof(implementationType));
        }

        if (!serviceType.IsAssignableFrom(implementationType))
        {
            throw new ArgumentException(
                $"'{implementationName}' is not assignable to '{serviceName}'.",
                nameof(implementationType));
        }
    }

    private static bool ImplementsOpenGeneric(Type implementation, Type openService)
    {
        if (openService.IsInterface)
        {
            foreach (Type contract in implementation.GetInterfaces())
            {
                if (contract.IsGenericType && contract.GetGenericTypeDefinition() == openService)
                {
                    return true;
                }
            }

            return false;
        }

        for (Type? current = implementation; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == openService)
            {
                return true;
            }
        }

        return false;
    }
}
