using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;

namespace VT2Lib.Core.Extensions;

internal static class PrimitiveReaderExtensions
{
    public static T ReadSerializable<T>(this in PrimitiveReader reader)
        where T : ISerializable<T>
    {
        return T.Deserialize(reader.BaseStream);
    }

    public static T ReadSerializable<T>(this in PrimitiveReader reader, ISerializer<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return serializer.Deserialize(reader.BaseStream);
    }

    public static T ReadStruct<T>(this in PrimitiveReader reader)
        where T : unmanaged
    {
        int sizeOfT = Unsafe.SizeOf<T>();
        using StackAllocHelper<byte> buffer = sizeOfT <= 256
            ? new(stackalloc byte[sizeOfT])
            : new(sizeOfT);

        reader.ReadBytes(buffer.Span);
        return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer.Span));
    }

    public static void ReadSpan<T>(this in PrimitiveReader reader, scoped Span<T> destination)
        where T : unmanaged
    {
        reader.ReadBytes(MemoryMarshal.AsBytes(destination));
    }

    public static bool ReadBoolByte(this in PrimitiveReader reader)
    {
        return reader.ReadByte() != 0;
    }

    public static bool ReadBoolIntLE(this in PrimitiveReader reader)
    {
        return reader.ReadInt32LE() != 0;
    }

    public static bool ReadBoolIntBE(this in PrimitiveReader reader)
    {
        return reader.ReadInt32BE() != 0;
    }
}