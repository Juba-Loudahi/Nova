namespace Nova.DI;

public sealed class ServiceProviderOptions
{
    public bool ValidateOnBuild { get; init; } = true;

    public bool ValidateScopes { get; init; } = true;
}
