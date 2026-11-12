using Nova.DI.Internal;

namespace Nova.DI;

public sealed class ScopeValidationException : DependencyInjectionException
{
    public ScopeValidationException(string message)
        : base(message)
    {
    }

    public static ScopeValidationException ResolvedFromRoot(Type requested, Type scoped)
    {
        ArgumentNullException.ThrowIfNull(requested);
        ArgumentNullException.ThrowIfNull(scoped);

        string scopedName = TypeNameHelper.GetDisplayName(scoped);
        if (requested == scoped)
        {
            return new ScopeValidationException(
                $"Cannot resolve the scoped service '{scopedName}' from the root provider. " +
                "Create a scope first with CreateScope() and resolve it from that scope.");
        }

        return new ScopeValidationException(
            $"Cannot resolve '{TypeNameHelper.GetDisplayName(requested)}' from the root provider: " +
            $"it depends on the scoped service '{scopedName}'. Create a scope first with CreateScope() and resolve it from that scope.");
    }
}
