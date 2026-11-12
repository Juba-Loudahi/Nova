namespace Nova.DI.Internal;

internal sealed class CallSiteChain
{
    private readonly List<(ServiceCacheKey Key, Type? Implementation)> _path = [];
    private readonly HashSet<ServiceCacheKey> _inProgress = [];

    public void Enter(ServiceCacheKey key, Type? implementationType)
    {
        if (!_inProgress.Add(key))
        {
            throw new CircularDependencyException(BuildCycle(key));
        }

        _path.Add((key, implementationType));
    }

    public void Exit(ServiceCacheKey key)
    {
        _inProgress.Remove(key);
        _path.RemoveAt(_path.Count - 1);
    }

    public string DescribePath()
        => string.Join(" -> ", _path.Select(static step => step.Implementation is null
            ? TypeNameHelper.GetDisplayName(step.Key.Identifier.ServiceType)
            : $"{TypeNameHelper.GetDisplayName(step.Key.Identifier.ServiceType)}({TypeNameHelper.GetDisplayName(step.Implementation)})"));

    private List<Type> BuildCycle(ServiceCacheKey repeated)
    {
        int start = _path.FindIndex(step => step.Key == repeated);
        List<Type> cycle = new(_path.Count - start + 1);

        for (int index = start; index < _path.Count; index++)
        {
            cycle.Add(_path[index].Key.Identifier.ServiceType);
        }

        cycle.Add(repeated.Identifier.ServiceType);
        return cycle;
    }
}
