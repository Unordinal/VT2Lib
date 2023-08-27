using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.Core.IO;

internal ref struct SpanReader
{
    private readonly Span<byte> _span;
    private int _pos;

    public SpanReader(Span<byte> span)
    {
        _span = span;
    }

    public byte ReadByte()
    {
        Debug.Assert(_pos + 1 <= _span.Length);
        return _span[_pos++];
    }

    public int ReadInt32()
    {
        Debug.Assert(_pos + 4 <= _span.Length);
        var result = BinaryPrimitives.ReadInt32LittleEndian(_span[_pos..]);
        _pos += 4;
        return result;
    }

    public uint ReadUInt32()
    {
        Debug.Assert(_pos + 4 <= _span.Length);
        var result = BinaryPrimitives.ReadUInt32LittleEndian(_span[_pos..]);
        _pos += 4;
        return result;
    }

    public T Read<T>() where T : unmanaged
    {
        Debug.Assert(_pos + Unsafe.SizeOf<T>() <= _span.Length);
        var result = MemoryMarshal.Read<T>(_span[_pos..]);
        _pos += Unsafe.SizeOf<T>();
        return result;
    }
}