using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Collections;

namespace VT2Lib.Core.Stingray.Extensions;

public static class PrimitiveReaderExtensions
{
    public static IDString32 ReadIDString32(this ref PrimitiveReader reader, IIDString32Provider? idStringProvider = null)
    {
        idStringProvider ??= IDStringRepository.Shared;
        uint id = reader.ReadUInt32LE();
        if (!idStringProvider.TryGet(id, out var idString))
            idString = new IDString32(id);

        return idString;
    }

    public static IDString64 ReadIDString64(this ref PrimitiveReader reader, IIDString64Provider? idStringProvider = null)
    {
        idStringProvider ??= IDStringRepository.Shared;
        ulong id = reader.ReadUInt64LE();
        if (!idStringProvider.TryGet(id, out var idString))
            idString = new IDString64(id);

        return idString;
    }

    public static ResourceLocator ReadResourceLocator(this ref PrimitiveReader reader, IIDString64Provider? idStringProvider = null)
    {
        idStringProvider ??= IDStringRepository.Shared;
        IDString64 type = reader.ReadIDString64(idStringProvider);
        IDString64 name = reader.ReadIDString64(idStringProvider);

        return new ResourceLocator(type, name);
    }
}