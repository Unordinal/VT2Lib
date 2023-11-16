using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;

namespace VT2Lib.Core.Stingray.IO.Serialization;

internal sealed class VertexBuffersSerializer : SerializerBase<VertexBuffer[]>
{
    public static VertexBuffersSerializer Default { get; } = new();

    public override void Serialize(Stream stream, VertexBuffer[] value)
    {
        var writer = new PrimitiveWriter(stream);

        writer.WriteInt32LE(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            VertexBuffer buffer = value[i];
            writer.WriteInt32LE(buffer.Data.Length);
            writer.WriteBytes(buffer.Data);
            writer.WriteUInt32LE((uint)buffer.Validity);
            writer.WriteUInt32LE((uint)buffer.StreamType);
            writer.WriteUInt32LE(buffer.Count);
            writer.WriteUInt32LE(buffer.Stride);
        }

        for (int i = 0; i < value.Length; i++)
        {
            VertexBuffer buffer = value[i];
            writer.WriteSerializable(buffer.Channel);
        }
    }

    public override VertexBuffer[] Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        int bufferCount = reader.ReadInt32LE();
        DebugEx.Assert(bufferCount >= 0);

        var result = new VertexBuffer[bufferCount];
        for (int i = 0; i < result.Length; i++)
        {
            int dataLength = reader.ReadInt32LE();
            DebugEx.Assert(dataLength >= 0);

            byte[] data = reader.ReadBytes(dataLength);
            Validity validity = (Validity)reader.ReadUInt32LE();
            StreamType streamType = (StreamType)reader.ReadUInt32LE();
            uint count = reader.ReadUInt32LE();
            uint stride = reader.ReadUInt32LE();

            result[i] = new VertexBuffer
            {
                Data = data,
                Validity = validity,
                StreamType = streamType,
                Count = count,
                Stride = stride,
                Channel = null!,
            };
        }

        int channelCount = reader.ReadInt32LE();
        DebugEx.Assert(channelCount >= 0);
        DebugEx.Assert(channelCount == bufferCount);

        for (int i = 0; i < channelCount; i++)
        {
            result[i].Channel = reader.ReadSerializable<Channel>();
        }

        return result;
    }
}