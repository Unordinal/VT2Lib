using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Collections.Extensions;

namespace VT2Lib.Core.Stingray.IO.Serialization;

public sealed class IDString64Serializer : SerializerBase<IDString64>
{
    public static IDString64Serializer Default { get; } = new();

    private readonly IIDString64Provider _idString64Provider;

    public IDString64Serializer(IIDString64Provider? idString64Provider = null)
    {
        _idString64Provider = idString64Provider ?? IDStringRepository.Shared;
    }

    public override void Serialize(Stream stream, IDString64 value)
    {
        var writer = new PrimitiveWriter(stream);
        writer.WriteUInt64LE(value.ID);
    }

    public override IDString64 Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);
        ulong id = reader.ReadUInt64LE();
        return _idString64Provider.GetOrCreate(id);
    }
}