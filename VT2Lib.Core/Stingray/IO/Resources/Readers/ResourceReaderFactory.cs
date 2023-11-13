using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VT2Lib.Core.Stingray.Resources.Bones;
using VT2Lib.Core.Stingray.Resources.Unit;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

// Jeez, talk about over-engineered.
// At least, that's what this feels like.
public sealed class ResourceReaderFactory
{
    public static ResourceReaderFactory SharedBinaryReaders { get; }

    private readonly ConcurrentDictionary<IDString64, IResourceReader> _factory = new();

    static ResourceReaderFactory()
    {
        SharedBinaryReaders = new ResourceReaderFactory();
        SharedBinaryReaders.TryRegister(new ResourceReader(BonesResource.ResourceID, BonesResourceV0.ReadBinary));
        SharedBinaryReaders.TryRegister(new VersionedResourceReader(UnitResource.ResourceID, UnitResource.BinaryReaders));
    }

    public bool TryRegister(IResourceReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return _factory.TryAdd(reader.ResourceID, reader);
    }

    public bool TryUnregister(IDString64 resourceID)
    {
        return _factory.TryRemove(resourceID, out _);
    }

    public bool TryGet(IDString64 resourceID, [NotNullWhen(true)] out IResourceReader? reader)
    {
        return _factory.TryGetValue(resourceID, out reader);
    }
}