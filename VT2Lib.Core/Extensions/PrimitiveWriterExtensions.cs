using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;

namespace VT2Lib.Core.Extensions;

internal static class PrimitiveWriterExtensions
{
    public static void WriteSerializable<T>(this ref PrimitiveWriter writer, T value)
        where T : ISerializable<T>
    {
        T.Serialize(writer.BaseStream, value);
    }

    public static void WriteSerializable<T>(this ref PrimitiveWriter writer, T value, ISerializer<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        serializer.Serialize(writer.BaseStream, value);
    }

    public static void WriteSerializableSpan<T>(this ref PrimitiveWriter writer, scoped ReadOnlySpan<T> value, ArraySerializer<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        serializer.Serialize(writer.BaseStream, value);
    }

    public static void WriteStruct<T>(this ref PrimitiveWriter writer, scoped in T value)
        where T : unmanaged
    {
        var span = new Span<T>(ref Unsafe.AsRef(in value));
        writer.WriteBytes(MemoryMarshal.AsBytes(span));
    }

    public static void WriteSpan<T>(this ref PrimitiveWriter writer, scoped ReadOnlySpan<T> value)
        where T : unmanaged
    {
        writer.WriteBytes(MemoryMarshal.AsBytes(value));
    }
}