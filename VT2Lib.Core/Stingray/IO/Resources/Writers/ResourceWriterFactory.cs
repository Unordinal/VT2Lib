using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VT2Lib.Core.Stingray.IO.Resources.Writers;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers;

internal class ResourceWriterFactory
{
    public static ResourceWriterFactory SharedBinaryWriters { get; }

    private readonly ConcurrentDictionary<IDString64, ResourceWriterDelegate> _factory = new();

    static ResourceWriterFactory()
    {
        SharedBinaryWriters = new ResourceWriterFactory();
        SharedBinaryWriters.TryRegister(BonesResource.ResourceID, new Bones.BinaryBonesResourceWriter());
    }

    public bool TryRegister(IDString64 resourceID, ResourceWriterDelegate writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        return _factory.TryAdd(resourceID, writer);
    }

    public bool TryUnregister(IDString64 resourceID)
    {
        return _factory.TryRemove(resourceID, out _);
    }

    public bool TryGet(IDString64 resourceID, [NotNullWhen(true)] out ResourceWriterDelegate? writer)
    {
        return _factory.TryGetValue(resourceID, out writer);
    }
}