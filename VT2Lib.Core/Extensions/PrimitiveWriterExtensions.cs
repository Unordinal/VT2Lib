using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;

namespace VT2Lib.Core.Extensions;

internal static class PrimitiveWriterExtensions
{
    public static void WriteSerializable<T>(this in PrimitiveWriter writer, T value)
        where T : ISerializable<T>
    {
        T.Serialize(writer.BaseStream, value);
    }

    public static void WriteSerializable<T>(this in PrimitiveWriter writer, T value, ISerializer<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        serializer.Serialize(writer.BaseStream, value);
    }

    public static void WriteSerializableSpan<T>(this in PrimitiveWriter writer, scoped ReadOnlySpan<T> value, ArraySerializer<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        serializer.Serialize(writer.BaseStream, value);
    }

    public static void WriteStruct<T>(this in PrimitiveWriter writer, scoped in T value)
        where T : unmanaged
    {
        var span = new Span<T>(ref Unsafe.AsRef(in value));
        writer.WriteBytes(MemoryMarshal.AsBytes(span));
    }

    public static void WriteSpan<T>(this in PrimitiveWriter writer, scoped ReadOnlySpan<T> value)
        where T : unmanaged
    {
        writer.WriteBytes(MemoryMarshal.AsBytes(value));
    }

    public static void WriteBoolByte(this in PrimitiveWriter writer, bool value)
    {
        writer.WriteByte((byte)(value ? 1 : 0));
    }

    public static void WriteBoolIntLE(this in PrimitiveWriter writer, bool value)
    {
        writer.WriteUInt32LE((uint)(value ? 1 : 0));
    }

    public static void WriteBoolIntBE(this in PrimitiveWriter writer, bool value)
    {
        writer.WriteUInt32BE((uint)(value ? 1 : 0));
    }
}