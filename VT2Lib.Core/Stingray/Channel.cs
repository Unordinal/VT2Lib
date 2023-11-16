using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;

namespace VT2Lib.Core.Stingray;

public sealed class Channel : ISerializable<Channel>
{
    public VertexComponent Component { get; set; }

    public ChannelType Type { get; set; }

    public uint Set { get; set; } // TEXCOORD_0, TEXCOORD_1, etc

    public uint Stream { get; set; }

    public byte IsInstance { get; set; }

    public override string ToString()
    {
        return $"{GetComponentWithSet()}<{Type.ToString().ToLowerInvariant()}>";
    }

    private string GetComponentWithSet()
    {
        return Component switch
        {
            VertexComponent.Texcoord or
            VertexComponent.Color or
            VertexComponent.BlendIndices or
            VertexComponent.BlendWeights => $"{Component}_{Set}".ToUpperInvariant(),
            _ => Component.ToString().ToUpperInvariant(),
        };
    }

    public static void Serialize(Stream stream, Channel value)
    {
        var writer = new PrimitiveWriter(stream);
        writer.WriteUInt32LE((uint)value.Component);
        writer.WriteUInt32LE((uint)value.Type);
        writer.WriteUInt32LE(value.Set);
        writer.WriteUInt32LE(value.Stream);
        writer.WriteByte(value.IsInstance);
    }

    public static Channel Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);
        return new Channel
        {
            Component = (VertexComponent)reader.ReadUInt32LE(),
            Type = (ChannelType)reader.ReadUInt32LE(),
            Set = reader.ReadUInt32LE(),
            Stream = reader.ReadUInt32LE(),
            IsInstance = reader.ReadByte(),
        };
    }
}