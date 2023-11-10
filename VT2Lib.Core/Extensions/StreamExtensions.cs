using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2Lib.Core.Collections;

namespace VT2Lib.Core.Extensions;

public static class StreamExtensions
{
    public static void Skip(this Stream stream, long count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (stream.CanSeek)
        {
            if (stream.Position + count >= stream.Length)
                throw new EndOfStreamException();

            stream.Position += count;
            return;
        }

        using StackAllocHelper<byte> buffer = count <= 256
            ? new(stackalloc byte[(int)count])
            : new((int)Math.Min(count, 4096));

        while (count > 0)
        {
            int bytesToRead = (int)Math.Min(buffer.Length, count);
            stream.ReadExactly(buffer.Span[..bytesToRead]);
            count -= bytesToRead;
        }
    }

    public static void ReadToEnd(this Stream stream)
    {
        using RentedArray<byte> buffer = new(4096);
        var bufSpan = buffer.Span;

        int bytesRead;
        while ((bytesRead = stream.Read(bufSpan)) != 0) { }
    }

    public static async ValueTask ReadToEndAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using RentedArray<byte> buffer = new(4096);
        var bufMem = buffer.AsMemory();

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(bufMem, cancellationToken)) != 0) { }
    }

    public static int ReadInt32(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(buffer));
    }
}