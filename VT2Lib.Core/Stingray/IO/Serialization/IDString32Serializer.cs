using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Collections.Extensions;

namespace VT2Lib.Core.Stingray.IO.Serialization;

public sealed class IDString32Serializer : SerializerBase<IDString32>
{
    public static IDString32Serializer Default { get; } = new();

    public IIDString32Provider Provider => _idString32Provider;

    private readonly IIDString32Provider _idString32Provider;

    public IDString32Serializer(IIDString32Provider? idString32Provider = null)
    {
        _idString32Provider = idString32Provider ?? IDStringRepository.Shared;
    }

    public override void Serialize(Stream stream, IDString32 value)
    {
        var writer = new PrimitiveWriter(stream);
        writer.WriteUInt32LE(value.ID);
    }

    public override IDString32 Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);
        uint id = reader.ReadUInt32LE();
        return _idString32Provider.GetOrCreate(id);
    }
}