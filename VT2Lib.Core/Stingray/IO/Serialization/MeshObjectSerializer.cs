using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Scene;

namespace VT2Lib.Core.Stingray.IO.Serialization;

public sealed class MeshObjectSerializer : SerializerBase<MeshObject>
{
    public static MeshObjectSerializer Default { get; } = new(SerializerFactory.Default.GetSerializer<IDString32>());

    private readonly ISerializer<IDString32> _idString32Serializer;

    public MeshObjectSerializer(ISerializer<IDString32> idString32Serializer)
    {
        ArgumentNullException.ThrowIfNull(idString32Serializer);
        _idString32Serializer = idString32Serializer;
    }

    public override void Serialize(Stream stream, MeshObject value)
    {
        var writer = new PrimitiveWriter(stream);

        writer.WriteSerializable(value.Name, _idString32Serializer);
        writer.WriteInt32LE(value.NodeIndex);
        writer.WriteUInt32LE(value.GeometryIndex);
        writer.WriteUInt32LE(value.SkinIndex);
        writer.WriteUInt32LE((uint)value.Flags);
        writer.WriteSerializable(value.BoundingVolume);
    }

    public override MeshObject Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        return new MeshObject
        {
            Name = reader.ReadSerializable(_idString32Serializer),
            NodeIndex = reader.ReadInt32LE(),
            GeometryIndex = reader.ReadUInt32LE(),
            SkinIndex = reader.ReadUInt32LE(),
            Flags = (RenderableFlags)reader.ReadUInt32LE(),
            BoundingVolume = reader.ReadSerializable<BoundingVolume>(),
        };
    }
}