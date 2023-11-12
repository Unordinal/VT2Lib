using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;

namespace VT2Lib.Core.IO;

/// <summary>
/// Provides primitive writing functionality by wrapping a stream and writing bytes to it.
/// Designed to be lightweight with minimal heap allocations.
/// <para/>
/// Members are explicit in naming to prevent accidental writing of the incorrect type.
/// </summary>
public readonly ref partial struct PrimitiveWriter
{
    public Stream BaseStream => _stream;

    private readonly Stream _stream;

    public PrimitiveWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("The given stream does not support writing.", nameof(stream));

        _stream = stream;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte value)
    {
        _stream.WriteByte(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSByte(sbyte value)
    {
        _stream.WriteByte((byte)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(scoped ReadOnlySpan<byte> buffer)
    {
        _stream.Write(buffer);
    }

    public void WriteNullTermString(scoped ReadOnlySpan<byte> value)
    {
        WriteBytes(value);
        WriteByte(0);
    }

    public void WriteNullTermString(scoped ReadOnlySpan<char> value)
    {
        int maxBytes = Encoding.UTF8.GetMaxByteCount(value.Length);
        using StackAllocHelper<byte> buffer = maxBytes <= 128
            ? new(stackalloc byte[maxBytes])
            : new(maxBytes);

        var status = Utf8.FromUtf16(value, buffer.Span, out int charsRead, out int bytesWritten);
        WriteNullTermString(buffer.Span[..bytesWritten]);
    }
}