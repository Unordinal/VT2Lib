using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.Core;

internal static class UnsafeEx
{
    public static unsafe ref byte GetPinnableReference<T>(T obj)
        where T : class
    {
        return ref Unsafe.AsRef<byte>(*(void**)Unsafe.AsPointer(ref obj));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte GetByteRef<T>(ref readonly T value)
        where T : unmanaged
    {
        return ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in value));
    }

    public static unsafe void BufferConvert<TIn, TOut>(ReadOnlySpan<TIn> source, Span<TOut> destination, ElementConvertDelegate<TIn, TOut> elementConvertDelegate)
    {
        ArgumentNullException.ThrowIfNull(elementConvertDelegate);

        int count = Math.Min(source.Length, destination.Length);
        for (int i = 0; i < count; i++)
            destination[i] = elementConvertDelegate(in source[i]);
    }

    public delegate TOut ElementConvertDelegate<TIn, TOut>(ref readonly TIn source);
}