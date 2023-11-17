using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.PhysX.Extensions;

public static class StreamExtensions
{
    public static T ReadStruct<T>(this Stream stream)
        where T : unmanaged
    {
        int sizeOfT = Unsafe.SizeOf<T>();
        Span<byte> buffer = stackalloc byte[sizeOfT];
        stream.ReadExactly(buffer);
        return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer));
    }

    public static void ReadStruct<T>(this Stream stream, ref T value)
        where T : unmanaged
    {
        int sizeOfT = Unsafe.SizeOf<T>();
        stream.ReadExactly(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref value), sizeOfT));
    }
}