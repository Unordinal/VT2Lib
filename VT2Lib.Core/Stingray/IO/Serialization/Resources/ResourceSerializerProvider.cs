using System.Collections.Concurrent;
using System.Diagnostics;
using VT2Lib.Core.Stingray.IO.Serialization.Resources.Bones;
using VT2Lib.Core.Stingray.IO.Serialization.Resources.Unit;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources;

public sealed class ResourceSerializerProvider
{
    public static ResourceSerializerProvider Default { get; }

    private readonly ConcurrentDictionary<IDString64, IResourceSerializer> _serializers;

    static ResourceSerializerProvider()
    {
        Default = new();
        Default.RegisterSerializer(BonesSerializer.Default);
        Default.RegisterSerializer(UnitResourceSerializer.Default);
    }

    public ResourceSerializerProvider()
    {
        _serializers = new();
    }

    public void RegisterSerializer(IResourceSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        if (_serializers.ContainsKey(serializer.ResourceID))
            Trace.TraceWarning("A resource serializer for type '{serializer.ResourceID}' is already registered. Replacing.");
        
        _serializers[serializer.ResourceID] = serializer;
    }

    public bool HasSerializer(IDString64 resourceID)
    {
        return _serializers.ContainsKey(resourceID);
    }

    public IResourceSerializer GetSerializer(IDString64 resourceID)
    {
        if (!_serializers.TryGetValue(resourceID, out var serializer))
            throw new ArgumentException($"No resource serializer registered for resource type '{resourceID}'");

        return serializer;
    }
}