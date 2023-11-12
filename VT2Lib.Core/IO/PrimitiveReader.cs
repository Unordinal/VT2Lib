using System.Runtime.CompilerServices;
using VT2Lib.Core.Collections;

namespace VT2Lib.Core.IO;

/// <summary>
/// Provides primitive reading functionality by wrapping a stream and reading bytes from it.
/// Designed to be lightweight with minimal heap allocations.
/// </summary>
public readonly ref partial struct PrimitiveReader
{
    public Stream BaseStream => _stream;

    private readonly Stream _stream;

    public PrimitiveReader(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("The given stream does not support reading.", nameof(stream));

        _stream = stream;
    }

    public byte ReadByte()
    {
        int b = _stream.ReadByte();
        if (b == -1)
            throw new EndOfStreamException();

        return (byte)b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ReadSByte()
    {
        return unchecked((sbyte)ReadByte());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes(int count)
    {
        byte[] buffer = new byte[count];
        ReadBytes(buffer);
        return buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadBytes(scoped Span<byte> buffer)
    {
        _stream.ReadExactly(buffer);
    }

    public string ReadNullTermString()
    {
        // Will rent an array if > 256 chars so we don't blow the stack or fail on unexpectedly high values
        using ValueList<char> list = new(stackalloc char[256]);
        byte b;
        while ((b = ReadByte()) > 0)
            list.Add((char)b);

        return new string(list.AsSpan());
    }
}