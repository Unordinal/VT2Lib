using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VT2Lib.Core.Stingray.IO.Resources.Writers;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers;

internal class ResourceWriterRepository
{
    public static ResourceWriterRepository SharedBinaryWriters { get; }

    private readonly ConcurrentDictionary<IDString64, IResourceWriter> _repo = new();

    static ResourceWriterRepository()
    {
        SharedBinaryWriters = new ResourceWriterRepository();
        SharedBinaryWriters.TryRegister(new ResourceWriter(BonesResource.ResourceID, BonesResource.BinaryWriter));
    }

    public bool TryRegister(IResourceWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        return _repo.TryAdd(writer.ResourceID, writer);
    }

    public bool TryUnregister(IDString64 resourceID)
    {
        return _repo.TryRemove(resourceID, out _);
    }

    public bool TryGet(IDString64 resourceID, [NotNullWhen(true)] out IResourceWriter? writer)
    {
        return _repo.TryGetValue(resourceID, out writer);
    }
}