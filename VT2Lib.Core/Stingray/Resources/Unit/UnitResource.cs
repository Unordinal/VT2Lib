using VT2Lib.Core.Stingray.IO.Resources.Readers;

namespace VT2Lib.Core.Stingray.Resources.Unit;

public abstract class UnitResource : Resource<UnitResource>
{
    public static IDString64 ResourceID { get; } = "unit";

    public static VersionedResourceReaderRepository BinaryReaders { get; }

    static UnitResource()
    {
        BinaryReaders = new VersionedResourceReaderRepository();
        BinaryReaders.TryRegister(186, UnitResourceV186.ReadBinary);

        /*foreach (var (type, version) in UnitResource.GetAllVersions())
            ReadersRepo.TryRegister(version, type.GetMethod("ReadBinary")!.CreateDelegate<ResourceReaderDelegate>());*/
    }
}