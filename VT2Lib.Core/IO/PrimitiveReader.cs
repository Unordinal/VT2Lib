using System.Runtime.CompilerServices;
using VT2Lib.Core.Collections;

namespace VT2Lib.Core.IO;

// EVAL: This could be changed into a 'ref struct' just by changing 'ReadBytes' to be 'ReadBytes(scoped Span<byte> buffer)' and removing the IDisposable interface.
// Do I want to do that? Consider the consequences this might have in terms of passing it around and such. If constructing it is fast enough, we might not care
// if we often have to make a new one to wrap streams. This would have an impact on extension methods as well, though apparently that is not an issue since C# 7.2
// since the parameter declaration 'this ref TStruct value' is now allowed for structs.
/// <summary>
/// Provides primitive reading functionality by wrapping a stream and reading bytes from it.
/// Designed to be lightweight with minimal heap allocations.
/// </summary>
public sealed partial class PrimitiveReader : IDisposable
{
    public Stream BaseStream => _stream;

    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    private bool _disposed;

    public PrimitiveReader(Stream stream)
        : this(stream, false)
    {
    }

    public PrimitiveReader(Stream stream, bool leaveOpen)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("The given stream does not support reading.", nameof(stream));

        _stream = stream;
        _leaveOpen = leaveOpen;
    }

    public byte ReadByte()
    {
        ThrowIfDisposed();

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
        ThrowIfDisposed();

        byte[] buffer = new byte[count];
        ReadBytes(buffer);
        return buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadBytes(Span<byte> buffer)
    {
        ThrowIfDisposed();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte PeekByte()
    {
        return Peek(ReadByte);
    }

    // EVAL: Is this really a good idea? Should it be kept private?
    private T Peek<T>(Func<T> action)
    {
        ThrowIfDisposed();
        if (!_stream.CanSeek)
            throw new NotSupportedException("The wrapped stream does not support seeking.");

        long origPos = _stream.Position;
        T result = action();
        _stream.Position = origPos;
        return result;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && !_leaveOpen)
            {
                _stream.Dispose();
            }

            _disposed = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}