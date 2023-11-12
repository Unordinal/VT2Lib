using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

public sealed class ResourceReaderFactory
{
    public static ResourceReaderFactory SharedBinaryReaders { get; }

    private readonly ConcurrentDictionary<IDString64, ResourceReaderDelegate> _factory = new();

    static ResourceReaderFactory()
    {
        SharedBinaryReaders = new ResourceReaderFactory();
        SharedBinaryReaders.TryRegister(BonesResource.ResourceID, new Bones.BinaryBonesResourceReader());
    }

    public bool TryRegister(IDString64 resourceID, ResourceReaderDelegate reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return _factory.TryAdd(resourceID, reader);
    }

    public bool TryUnregister(IDString64 resourceID)
    {
        return _factory.TryRemove(resourceID, out _);
    }

    public bool TryGet(IDString64 resourceID, [NotNullWhen(true)] out ResourceReaderDelegate? reader)
    {
        return _factory.TryGetValue(resourceID, out reader);
    }
}