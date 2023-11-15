using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.IO.Resources.Readers;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources;

public class VersionedResourceReader : IResourceReader
{
    public IDString64 ResourceID { get; }

    public VersionedResourceReaderRepository VersionedReaderRepo { get; }

    public VersionedResourceReader(IDString64 resourceID, VersionedResourceReaderRepository versionedReaderRepo)
    {
        ResourceID = resourceID;
        VersionedReaderRepo = versionedReaderRepo;
    }

    public bool TryRegisterReaderVersion(int resourceVersion, ResourceReaderDelegate readerFunc)
    {
        return VersionedReaderRepo.TryRegister(resourceVersion, readerFunc);
    }

    public bool TryUnregisterReaderVersion(int resourceVersion)
    {
        return VersionedReaderRepo.TryUnregister(resourceVersion);
    }

    public bool CanRead(IDString64 resourceID)
    {
        return resourceID == ResourceID;
    }

    public bool CanRead(IDString64 resourceID, int resourceVersion)
    {
        return CanRead(resourceID) && VersionedReaderRepo.HasReaderForVersion(resourceVersion);
    }

    public IResource Read(Stream stream)
    {
        var reader = new PrimitiveReader(stream);
        int version = reader.ReadInt32LE();
        if (!VersionedReaderRepo.TryGet(version, out var readerFunc))
            throw new NotSupportedException($"No reader registered for version {version} of resource '{ResourceID}'.");

        IResource resource = readerFunc(in reader);
        if (resource.GetResourceID() != ResourceID)
            throw new InvalidOperationException($"Mismatched type read for '{ResourceID}' reader.");

        return resource;
    }
}