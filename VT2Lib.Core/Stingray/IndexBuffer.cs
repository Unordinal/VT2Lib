using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;

namespace VT2Lib.Core.Stingray;

public sealed class IndexBuffer : ISerializable<IndexBuffer>
{
    public required Validity Validity { get; set; }

    public required StreamType StreamType { get; set; }

    public required IndexFormat IndexFormat { get; set; }

    public required uint IndexCount { get; set; }

    public required byte[] Data { get; set; }

    public override string ToString()
    {
        return $"{nameof(IndexBuffer)}<{IndexFormat}>[{IndexCount}]";
    }

    public static void Serialize(Stream stream, IndexBuffer value)
    {
        var writer = new PrimitiveWriter(stream);
        writer.WriteUInt32LE((uint)value.Validity);
        writer.WriteUInt32LE((uint)value.StreamType);
        writer.WriteUInt32LE((uint)value.IndexFormat);
        writer.WriteUInt32LE(value.IndexCount);
        writer.WriteInt32LE(value.Data.Length);
        writer.WriteBytes(value.Data);
    }

    public static IndexBuffer Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        Validity validity = (Validity)reader.ReadUInt32LE();
        StreamType streamType = (StreamType)reader.ReadUInt32LE();
        IndexFormat indexFormat = (IndexFormat)reader.ReadUInt32LE();
        uint count = reader.ReadUInt32LE();

        int indicesLength = reader.ReadInt32LE();
        DebugEx.Assert(indicesLength >= 0);

        byte[] indices = reader.ReadBytes(indicesLength);
        return new IndexBuffer
        {
            Validity = validity,
            StreamType = streamType,
            IndexFormat = indexFormat,
            IndexCount = count,
            Data = indices,
        };
    }
}