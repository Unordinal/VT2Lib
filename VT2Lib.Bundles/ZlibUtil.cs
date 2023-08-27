﻿using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Core.Collections;

namespace VT2Lib.Bundles;

internal static class ZlibUtil
{
    public static int Decompress(Inflater inflater, byte[] input, int start, int count, Span<byte> destination)
    {
        ArgumentNullException.ThrowIfNull(inflater);
        ArgumentNullException.ThrowIfNull(input);

        inflater.SetInput(input, start, count);
        using RentedArray<byte> buffer = new(destination.Length);

        int totalBytesDecompressed = 0;
        while (!inflater.IsFinished)
        {
            int bytesDecompressed = inflater.Inflate(buffer.RawArrayUnsafe, 0, buffer.Length);
            totalBytesDecompressed += bytesDecompressed;
            if (bytesDecompressed > destination.Length)
                throw new ArgumentException($"The destination is not big enough to contain the decompressed bytes. ({totalBytesDecompressed} > {destination.Length})");

            buffer.AsSpan(0, bytesDecompressed).CopyTo(destination);
            if (bytesDecompressed < destination.Length)
                destination = destination[bytesDecompressed..];
        }

        return totalBytesDecompressed;
    }

    public static unsafe int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        fixed (byte* pSource = source)
        {
            using var stream = new UnmanagedMemoryStream(pSource, source.Length);
            using var zlibStream = new ZLibStream(stream, CompressionMode.Decompress);

            return zlibStream.ReadAtLeast(destination, destination.Length, false);
        }
    }
    
    public static unsafe ValueTask<int> DecompressAsync(ReadOnlyMemory<byte> source, Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled<int>(cancellationToken);

        fixed (byte* pSource = source.Span)
        {
            using var stream = new UnmanagedMemoryStream(pSource, source.Length);
            using var zlibStream = new ZLibStream(stream, CompressionMode.Decompress);

            return zlibStream.ReadAtLeastAsync(destination, destination.Length, false, cancellationToken);
        }
    }
}
