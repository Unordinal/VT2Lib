using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Scene;

namespace VT2Lib.Core.Stingray.IO.Serialization;

internal sealed class MeshGeometrySerializer : SerializerBase<MeshGeometry>
{
    public static MeshGeometrySerializer Default { get; } = new(SerializerFactory.Default.GetSerializer<IDString32>());

    private readonly ISerializer<IDString32> _idString32Serializer;
    private readonly ArraySerializer<BatchRange> _batchRangesSerializer;
    private readonly ArraySerializer<IDString32> _materialsSerializer;

    public MeshGeometrySerializer(ISerializer<IDString32> idString32Serializer)
    {
        ArgumentNullException.ThrowIfNull(idString32Serializer);
        _idString32Serializer = idString32Serializer;
        _batchRangesSerializer = ArraySerializer.Create<BatchRange>();
        _materialsSerializer = ArraySerializer.Create(_idString32Serializer);
    }

    public override void Serialize(Stream stream, MeshGeometry value)
    {
        var writer = new PrimitiveWriter(stream);

        writer.WriteSerializable(value.VertexBuffers, VertexBuffersSerializer.Default);
        writer.WriteSerializable(value.IndexBuffer);
        writer.WriteSerializable(value.BatchRanges, _batchRangesSerializer);
        writer.WriteSerializable(value.BoundingVolume);
        writer.WriteSerializable(value.Materials, _materialsSerializer);
    }

    public override MeshGeometry Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        return new MeshGeometry
        {
            VertexBuffers = reader.ReadSerializable(VertexBuffersSerializer.Default),
            IndexBuffer = reader.ReadSerializable<IndexBuffer>(),
            BatchRanges = reader.ReadSerializable(_batchRangesSerializer),
            BoundingVolume = reader.ReadSerializable<BoundingVolume>(),
            Materials = reader.ReadSerializable(_materialsSerializer),
        };
    }
}