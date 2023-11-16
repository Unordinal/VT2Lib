using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;

namespace VT2Lib.Core.Stingray;

public sealed class BatchRange : ISerializable<BatchRange>
{
    public required uint MaterialIndex { get; set; }

    /// <summary>
    /// Per-triangle; multiply by 3 to get the index.
    /// </summary>
    public required uint Start { get; set; }

    /// <summary>
    /// Per-triangle; multiply by 3 to get the index.
    /// </summary>
    public required uint Size { get; set; }

    public required uint BoneSet { get; set; }

    public static void Serialize(Stream stream, BatchRange value)
    {
        var writer = new PrimitiveWriter(stream);

        writer.WriteUInt32LE(value.MaterialIndex);
        writer.WriteUInt32LE(value.Start);
        writer.WriteUInt32LE(value.Size);
        writer.WriteUInt32LE(value.BoneSet);
    }

    public static BatchRange Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        return new BatchRange
        {
            MaterialIndex = reader.ReadUInt32LE(),
            Start = reader.ReadUInt32LE(),
            Size = reader.ReadUInt32LE(),
            BoneSet = reader.ReadUInt32LE(),
        };
    }
}