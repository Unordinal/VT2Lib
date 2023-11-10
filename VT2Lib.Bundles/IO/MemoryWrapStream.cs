using System.Runtime.InteropServices;

namespace VT2Lib.Bundles.IO;

internal sealed class MemoryWrapStream : Stream
{
    public override bool CanRead => true;

    public override bool CanWrite => _isReadOnly == false;

    public override bool CanSeek => true;

    public bool IsReadOnly => _isReadOnly;

    public override long Length => _memory.Length;

    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    private readonly Memory<byte> _memory;
    private readonly bool _isReadOnly;
    private int _position;

    public MemoryWrapStream(Memory<byte> memory)
    {
        _memory = memory;
    }

    public MemoryWrapStream(ReadOnlyMemory<byte> memory)
    {
        _memory = MemoryMarshal.AsMemory(memory);
        _isReadOnly = true;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (offset is < int.MinValue or > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if (origin == SeekOrigin.Current)
            offset = _position + offset;

        if (offset < 0 || ((ulong)offset >= (ulong)(uint)_memory.Length))
            throw new ArgumentOutOfRangeException(nameof(offset));

        Index index = new((int)offset, origin == SeekOrigin.End);
        return _position = index.GetOffset(_memory.Length);
    }

    public override int ReadByte()
    {
        return _position < _memory.Length ? _memory.Span[_position++] : -1;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        int maxLen = Math.Min(_memory.Length - _position, buffer.Length);
        _memory.Span.Slice(_position, maxLen).CopyTo(buffer);
        return maxLen;
    }

    public override void WriteByte(byte value)
    {
        ThrowIfReadOnly();
        _memory.Span[_position++] = value;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ThrowIfReadOnly();
        ValidateBufferArguments(buffer, offset, count);
        Write(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ThrowIfReadOnly();
        buffer.CopyTo(_memory.Span[_position..]);
        _position += buffer.Length;
    }

    public override void Flush()
    {
        // no-op
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    private void ThrowIfReadOnly()
    {
        if (_isReadOnly)
            throw new InvalidOperationException($"Cannot write to an instance of {nameof(MemoryWrapStream)} that was created with a {nameof(ReadOnlyMemory<byte>)}.");
    }
}