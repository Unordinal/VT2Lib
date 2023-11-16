using System.Numerics;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;

namespace VT2Lib.Core.Stingray;

public readonly record struct BoundingVolume : ISerializable<BoundingVolume>
{
    public required Vector3 LowerBounds { get; init; }

    public required Vector3 UpperBounds { get; init; }

    public required float Radius { get; init; }

    public static void Serialize(Stream stream, BoundingVolume value)
    {
        var writer = new PrimitiveWriter(stream);

        writer.WriteVector3LE(value.LowerBounds);
        writer.WriteVector3LE(value.UpperBounds);
        writer.WriteSingleLE(value.Radius);
    }

    public static BoundingVolume Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        return new BoundingVolume
        {
            LowerBounds = reader.ReadVector3LE(),
            UpperBounds = reader.ReadVector3LE(),
            Radius = reader.ReadSingleLE()
        };
    }
}