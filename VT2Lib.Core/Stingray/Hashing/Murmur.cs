﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace VT2Lib.Core.Stingray.Hashing;

/// <summary>
/// Provides methods relating to Murmur2 hashing. Uses Murmur64A.
/// </summary>
/// <remarks>
///     The 32-bit methods used are specific to the Stingray engine; they actually use the 64-bit hashing functions and
///     just shift and chop off half the bytes before casting to <see cref="uint"/>.
///     <para/>
///     The unsafe functions with pointer manipulation in this class could be replaced with
///     <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/> calls instead if the 'unsafe' flag is undesired.
/// </remarks>
public static class Murmur
{
    /// <summary>
    /// Hashes the specified value as a Murmur64A hash.
    /// </summary>
    /// <param name="key">The value to hash.</param>
    /// <returns>A 64-bit Murmur64A hash of <paramref name="key"/>.</returns>
    public static ulong Hash64(ReadOnlySpan<byte> key, ulong seed = 0)
    {
        return Murmur64(key, seed);
    }

    /// <summary>
    /// Hashes the specified value as a Murmur64A hash.
    /// </summary>
    /// <param name="key">The string to hash.</param>
    /// <returns>A 64-bit Murmur64A hash of <paramref name="key"/>.</returns>
    public static ulong Hash64(string key, ulong seed = 0)
    {
        ArgumentNullException.ThrowIfNull(key);

        const int MaxAllocSize = 256;
        var source = key.AsSpan();
        int destLength = (source.Length + 1) * 3; // The worst case scenario for 'UTF-16' -> 'UTF-8'.
        using StackAllocHelper<byte> buffer = destLength <= MaxAllocSize
                                            ? new(stackalloc byte[destLength])
                                            : new(destLength);

        // This method is more optimized than 'UTF8Encoding.GetBytes' and does not allocate an array, which is good when this hash function will be called often.
        var conversionResult = Utf8.FromUtf16(source, buffer.Span, out _, out int bytesWritten);
        Debug.Assert(conversionResult == System.Buffers.OperationStatus.Done);

        return Murmur64(buffer.Span[..bytesWritten], seed);
    }

    /// <summary>
    /// Hashes the specified value as a Murmur64A hash and converts the result to a 32-bit Stingray Murmur hash.
    /// </summary>
    /// <param name="key">The value to hash.</param>
    /// <returns>A 32-bit Murmur64A hash of <paramref name="key"/>.</returns>
    public static uint Hash32(ReadOnlySpan<byte> key, uint seed = 0)
    {
        ulong hash64 = Hash64(key, seed);
        return GetAsHash32(hash64);
    }

    /// <summary>
    /// Hashes the specified value as a Murmur64A hash and converts the result to a 32-bit Stingray Murmur hash.
    /// </summary>
    /// <param name="key">The string to hash.</param>
    /// <returns>A 32-bit Murmur64A hash of <paramref name="key"/>.</returns>
    public static uint Hash32(string key, uint seed = 0)
    {
        ArgumentNullException.ThrowIfNull(key);

        ulong hash64 = Hash64(key, seed);
        return GetAsHash32(hash64);
    }

    /// <summary>
    /// Converts the specified 64-bit Murmur hash to a Stingray 32-bit Murmur hash.
    /// </summary>
    /// <param name="hash">The hash value to convert.</param>
    /// <returns><paramref name="hash"/> as a 32-bit Stingray Murmur hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetAsHash32(ulong hash)
    {
        return (uint)(hash >> 32);
    }

    private static unsafe ulong Murmur64(ReadOnlySpan<byte> key, ulong seed)
    {
        const ulong M = 0xc6a4a7935bd1e995ul;
        const int R = 47;

        int length = key.Length;
        ulong h = seed ^ (ulong)key.Length * M;

        // Is this better or worse than &MemoryMarshal.GetReference(key)?
        // IIRC GetRef always returns a non-null pointer even on empty spans - is that desired?
        // I don't believe we care either way here as we never dereference it unless the length is > 0.
        fixed (byte* pKey = key)
        {
            ulong* pData = (ulong*)pKey;
            ulong* pEnd = pData + length / sizeof(ulong);

            while (pData != pEnd)
            {
                ulong k = *pData++;

                k *= M;
                k ^= k >> R;
                k *= M;

                h ^= k;
                h *= M;
            }

            byte* pData2 = (byte*)pData;

            int remaining = length % sizeof(ulong);
            while (remaining-- > 0)
            {
                int index = remaining;
                int shiftBy = index * 8;
                h ^= (ulong)pData2[index] << shiftBy;

                if (remaining == 0)
                    h *= M;
            }
        }

        h ^= h >> R;
        h *= M;
        h ^= h >> R;

        return h;
    }

    private static unsafe ulong EightByteHash64(ReadOnlySpan<byte> key)
    {
        const ulong M = 0xc6a4a7935bd1e995ul;
        const int R = 47;

        fixed (byte* pKey = key)
        {
            ulong k = *(ulong*)pKey;

            k *= M;
            k ^= k >> R;
            k *= M;

            return k;
        }
    }

    private static unsafe uint FourByteHash32(ReadOnlySpan<byte> key)
    {
        const uint M = 0x5bd1e995;
        const int R = 24;

        fixed (byte* pKey = key)
        {
            uint k = *(uint*)pKey;

            k *= M;
            k ^= k >> R;
            k *= M;

            return k;
        }
    }
}