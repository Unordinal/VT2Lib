using System.Runtime.InteropServices;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Collections;

namespace VT2Lib.Core.Stingray.IO.Serialization;

public sealed class SimpleAnimationGroupSerializer : SerializerBase<SimpleAnimationGroup>
{
    public static SimpleAnimationGroupSerializer Default { get; } = new(SerializerFactory.Default.GetSerializer<IDString32>());

    private readonly ISerializer<IDString32> _idString32Serializer;

    public SimpleAnimationGroupSerializer(ISerializer<IDString32> idString32Serializer)
    {
        ArgumentNullException.ThrowIfNull(idString32Serializer);
        _idString32Serializer = idString32Serializer;
    }

    public override void Serialize(Stream stream, SimpleAnimationGroup value)
    {
        var writer = new PrimitiveWriter(stream);

        writer.WriteSerializable(value.Name, _idString32Serializer);
        writer.WriteInt32LE(value.Data.Length);
        writer.WriteBytes(MemoryMarshal.AsBytes<int>(value.Data));
    }

    public override SimpleAnimationGroup Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        IDString32 name = reader.ReadSerializable(_idString32Serializer);
        int dataLength = reader.ReadInt32LE();
        int[] data = new int[dataLength];
        reader.ReadBytes(MemoryMarshal.AsBytes<int>(data));

        return new SimpleAnimationGroup
        {
            Name = name,
            Data = data,
        };
    }
}