using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Resources.Unit;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers.Unit;

public class BinaryUnitResourceReader : ResourceReader<UnitResource>
{
    public BinaryUnitResourceReader()
    {
        TryRegisterReaderVersion(186, UnitResourceV186.ReadBinary);
    }

    public override UnitResource Read(Stream stream)
    {
        PrimitiveReader reader = new(stream);
        int version = reader.ReadInt32LE();

        if (!VersionedReaderRepo.TryGet(version, out var versionedReader))
            throw new NotSupportedException($"No reader found for resource '{nameof(UnitResource)}', version '{version}'");

        return (UnitResource)versionedReader(ref reader);
    }
}
