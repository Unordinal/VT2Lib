using VT2Lib.Core.Stingray.IO.Resources.Readers;
using VT2Lib.Core.Stingray.IO.Resources.Writers;
using VT2Lib.Core.Stingray.Scene;

namespace VT2Lib.Core.Stingray.Resources.Unit;

public abstract class UnitResource : Resource<UnitResource>
{
    public static IDString64 ResourceID { get; } = "unit";

    public static VersionedResourceReaderRepository BinaryReaders { get; }

    public static VersionedResourceWriterRepository BinaryWriters { get; }

    public required MeshGeometry[] MeshGeometries { get; set; }

    static UnitResource()
    {
        BinaryReaders = new VersionedResourceReaderRepository();
        BinaryReaders.TryRegister(186, UnitResourceV186.ReadBinary);
        
        BinaryWriters = new VersionedResourceWriterRepository();
        BinaryWriters.TryRegister(186, UnitResourceV186.WriteBinary);

        /*foreach (var (type, version) in UnitResource.GetAllVersions())
            ReadersRepo.TryRegister(version, type.GetMethod("ReadBinary")!.CreateDelegate<ResourceReaderDelegate>());*/
    }
}