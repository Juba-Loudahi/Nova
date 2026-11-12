using System.Reflection;

namespace Nova.DI.Internal;

internal sealed class ConstructorSelector
{
    private readonly Func<Type, bool> _isService;

    public ConstructorSelector(Func<Type, bool> isService)
    {
        _isService = isService;
    }

    public ConstructorInfo Select(Type implementation)
    {
        ConstructorInfo[] constructors = implementation.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        if (constructors.Length == 0)
        {
            throw ResolutionException.NoPublicConstructor(implementation);
        }

        if (constructors.Length == 1)
        {
            return constructors[0];
        }

        ConstructorInfo? marked = null;
        foreach (ConstructorInfo constructor in constructors)
        {
            if (!constructor.IsDefined(typeof(InjectionConstructorAttribute), inherit: false))
            {
                continue;
            }

            if (marked is not null)
            {
                throw ResolutionException.MultipleInjectionConstructors(implementation);
            }

            marked = constructor;
        }

        if (marked is not null)
        {
            return marked;
        }

        (ConstructorInfo Constructor, ParameterInfo[] Parameters)[] candidates = constructors
            .Select(static constructor => (constructor, constructor.GetParameters()))
            .OrderByDescending(static candidate => candidate.Item2.Length)
            .ToArray();

        ConstructorInfo? best = null;
        int bestLength = -1;
        bool ambiguous = false;

        foreach ((ConstructorInfo constructor, ParameterInfo[] parameters) in candidates)
        {
            if (best is not null && parameters.Length < bestLength)
            {
                break;
            }

            if (!IsSatisfiable(parameters))
            {
                continue;
            }

            if (best is null)
            {
                best = constructor;
                bestLength = parameters.Length;
            }
            else
            {
                ambiguous = true;
            }
        }

        if (best is null)
        {
            throw ResolutionException.NoSatisfiableConstructor(implementation);
        }

        if (ambiguous)
        {
            throw ResolutionException.AmbiguousConstructors(implementation, bestLength);
        }

        return best;
    }

    private bool IsSatisfiable(ParameterInfo[] parameters)
    {
        foreach (ParameterInfo parameter in parameters)
        {
            if (!parameter.HasDefaultValue && !_isService(parameter.ParameterType))
            {
                return false;
            }
        }

        return true;
    }
}
