namespace Nova.DI.Internal;

internal readonly record struct ServiceCacheKey(ServiceIdentifier Identifier, int DescriptorIndex)
{
    public const int NoDescriptor = -1;
}
