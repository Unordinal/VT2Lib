using System.Collections.Concurrent;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources;

public sealed class VersionedResourceSerializerProvider<TResource>
    where TResource : IResource
{
    public IDString64 ResourceID { get; }

    private readonly ConcurrentDictionary<int, ResourceSerializer<TResource>> _serializers;

    public VersionedResourceSerializerProvider(IDString64 resourceID)
    {
        ResourceID = resourceID;
        _serializers = new();
    }

    public void RegisterSerializer(int version, ResourceSerializer<TResource> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        if (serializer.ResourceID != ResourceID)
            throw new ArgumentException($"Serializer {serializer.GetType().Name} with type '{serializer.ResourceID}' does not support resource '{ResourceID}'");

        _serializers[version] = serializer;
    }

    public bool HasSerializer(int version)
    {
        return _serializers.ContainsKey(version);
    }

    public ResourceSerializer<TResource> GetSerializer(int version)
    {
        return _serializers[version];
    }
}