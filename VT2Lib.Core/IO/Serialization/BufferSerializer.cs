using System.Runtime.InteropServices;
using VT2Lib.Core.Extensions;

namespace VT2Lib.Core.IO.Serialization;

public sealed class BufferSerializer<T> : SerializerBase<T[]>
    where T : unmanaged
{
    public static BufferSerializer<T> Default { get; } = new();

    public override void Serialize(Stream stream, T[] value)
    {
        Serialize(stream, value.AsSpan());
    }

    public void Serialize(Stream stream, scoped ReadOnlySpan<T> value)
    {
        stream.WriteInt32LE(value.Length);
        stream.Write(MemoryMarshal.AsBytes(value));
    }

    public override T[] Deserialize(Stream stream)
    {
        int length = stream.ReadInt32LE();
        T[] result = new T[length];
        stream.ReadExactly(MemoryMarshal.AsBytes<T>(result));
        return result;
    }
}