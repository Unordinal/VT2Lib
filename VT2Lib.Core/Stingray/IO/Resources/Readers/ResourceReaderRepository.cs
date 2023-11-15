using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VT2Lib.Core.Stingray.Resources.Bones;
using VT2Lib.Core.Stingray.Resources.Unit;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

// Jeez, talk about over-engineered.
// At least, that's what this feels like.
public sealed class ResourceReaderRepository
{
    public static ResourceReaderRepository SharedBinaryReaders { get; }

    private readonly ConcurrentDictionary<IDString64, IResourceReader> _repo = new();

    static ResourceReaderRepository()
    {
        SharedBinaryReaders = new ResourceReaderRepository();
        SharedBinaryReaders.TryRegister(new ResourceReader(BonesResource.ResourceID, BonesResource.BinaryReader));
        SharedBinaryReaders.TryRegister(new VersionedResourceReader(UnitResource.ResourceID, UnitResource.BinaryReaders));
    }

    public bool TryRegister(IResourceReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return _repo.TryAdd(reader.ResourceID, reader);
    }

    public bool TryUnregister(IDString64 resourceID)
    {
        return _repo.TryRemove(resourceID, out _);
    }

    public bool TryGet(IDString64 resourceID, [NotNullWhen(true)] out IResourceReader? reader)
    {
        return _repo.TryGetValue(resourceID, out reader);
    }
}