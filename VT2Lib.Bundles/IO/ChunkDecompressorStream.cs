using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using VT2Lib.Core.Collections;

namespace VT2Lib.Bundles.IO;

// _uncompressedPos is not supported as the position starts at zero when this instance is contstructed.
// This would be fine if we could assume that the wrapped stream's position is zero, but we can't.
// Another option would be to initialize _uncompressedPos with the stream's position when this instance
// is constructed, but this would be incorrect if there was compressed data previously in the stream and would be
// an invalid position.
// We could have a constructor that takes in an initial position, but this would be awkward and make for an
// unclear API. If users need the position, they can track it themselves.
internal class ChunkDecompressorStream : Stream
{
    /// <summary>
    /// Returns whether the decompressor stream supports reading.
    /// </summary>
    /// <returns><see langword="true"/> if the stream has not been disposed; otherwise, <see langword="false"/>.</returns>
    public override bool CanRead => !_disposed;

    /// <summary>
    /// Gets a value indicating whether the stream supports writing.
    /// </summary>
    /// <returns><see langword="false"/> in all cases.</returns>
    public override bool CanWrite => false;

    /// <summary>
    /// Gets a value indicating whether the stream supports seeking.
    /// </summary>
    /// <returns><see langword="false"/> in all cases.</returns>
    public override bool CanSeek => false;

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    public override long Length => throw new NotSupportedException();

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    public override long Position
    {
        get => throw new NotSupportedException(); //_uncompressedPos;
        set => throw new NotSupportedException();
    }

    private readonly Stream _stream;
    private readonly int _maxChunkLength;
    private readonly int _numChunksToBuffer;
    private readonly bool _leaveOpen;
    //private long _uncompressedPos; // The position in the stream as if this was an uncompressed stream.
    private bool _disposed;

    private readonly RentedArray<byte> _compressedBuffer; // Temporarily holds read compressed data to be decompressed into _buffer
    private readonly RentedArray<byte> _buffer;
    private int _readPos;
    private int _readLen;

    public ChunkDecompressorStream(Stream stream, int maxChunkLength, int numChunksToBuffer, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("The passed stream does not support reading.");
        if (maxChunkLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxChunkLength));
        if (numChunksToBuffer <= 0)
            throw new ArgumentOutOfRangeException(nameof(numChunksToBuffer));

        _stream = stream;
        _maxChunkLength = maxChunkLength;
        _numChunksToBuffer = numChunksToBuffer;
        _leaveOpen = leaveOpen;

        _compressedBuffer = new RentedArray<byte>(maxChunkLength);
        _buffer = new RentedArray<byte>(maxChunkLength * numChunksToBuffer);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        ValidateBufferArguments(buffer, offset, count);
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        ThrowIfDisposed();

        int bytesFromBuffer = ReadFromBuffer(buffer);
        if (bytesFromBuffer == buffer.Length)
            return bytesFromBuffer;

        int bytesFromNewChunks = FillBuffer();
        if (bytesFromNewChunks == 0)
            return bytesFromBuffer;

        return ReadFromBuffer(buffer[bytesFromBuffer..]) + bytesFromBuffer;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateBufferArguments(buffer, offset, count);
        return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        int bytesFromBuffer = ReadFromBuffer(buffer.Span);
        if (bytesFromBuffer == buffer.Length)
            return bytesFromBuffer;

        int bytesFromNewChunks = await FillBufferAsync(cancellationToken).ConfigureAwait(false);
        if (bytesFromNewChunks == 0)
            return bytesFromBuffer;

        return ReadFromBuffer(buffer.Span[bytesFromBuffer..]) + bytesFromBuffer;
    }

    public override void Flush()
    {
        // No-op. I'm assuming there's no reason for this to throw.
    }

    private int ReadFromBuffer(Span<byte> destination)
    {
        int bytesToCopy = Math.Min(_readLen - _readPos, destination.Length);
        Debug.Assert(bytesToCopy >= 0);

        if (bytesToCopy > 0)
        {
            _buffer.AsSpan(_readPos, bytesToCopy).CopyTo(destination);
            _readPos += bytesToCopy;
            //_uncompressedPos += bytesToCopy;
        }

        return bytesToCopy;
    }

    private int FillBuffer()
    {
        Debug.Assert(_readPos == _readLen);

        int totalBytesRead = 0;
        for (int i = 0; i < _numChunksToBuffer; i++)
        {
            int bytesRead = ReadAndDecompressChunk(_buffer.AsSpan(totalBytesRead));
            if (bytesRead == 0)
                break;

            totalBytesRead += bytesRead;
        }

        _readPos = 0;
        _readLen = totalBytesRead;
        return totalBytesRead;
    }

    private async ValueTask<int> FillBufferAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(_readPos == _readLen);
        cancellationToken.ThrowIfCancellationRequested();

        int totalBytesRead = 0;
        for (int i = 0; i < _numChunksToBuffer; i++)
        {
            int bytesRead = await ReadAndDecompressChunkAsync(_buffer.AsMemory(totalBytesRead), cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
                break;

            totalBytesRead += bytesRead;
        }

        _readPos = 0;
        _readLen = totalBytesRead;
        return totalBytesRead;
    }

    private int ReadAndDecompressChunk(Span<byte> destination)
    {
        Debug.Assert(destination.Length >= _maxChunkLength);
        if (!TryReadChunkLength(out int chunkLength))
            return 0;

        var destSlice = destination[.._maxChunkLength];
        if (chunkLength == _maxChunkLength)
        {
            _stream.ReadExactly(destSlice); // No decompression needed.
            return chunkLength;
        }
        else
        {
            Span<byte> compBufferSlice = _compressedBuffer.AsSpan(0, chunkLength);
            _stream.ReadExactly(compBufferSlice);
            int bytesDecompressed = ZlibUtil.Decompress(compBufferSlice, destSlice);
            return bytesDecompressed;
        }
    }

    private async ValueTask<int> ReadAndDecompressChunkAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        Debug.Assert(destination.Length >= _maxChunkLength);
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryReadChunkLength(out int chunkLength))
            return 0;

        var destSlice = destination[.._maxChunkLength];
        if (chunkLength == _maxChunkLength)
        {
            await _stream.ReadExactlyAsync(destSlice, cancellationToken).ConfigureAwait(false);
            return chunkLength;
        }
        else
        {
            Memory<byte> compBufferSlice = _compressedBuffer.AsMemory(0, chunkLength);
            await _stream.ReadExactlyAsync(compBufferSlice, cancellationToken).ConfigureAwait(false);
            int bytesDecompressed = await ZlibUtil.DecompressAsync(compBufferSlice, destSlice, cancellationToken).ConfigureAwait(false);
            return bytesDecompressed;
        }
    }

    // EVAL: Should this never throw instead? Should it reset stream position on fail?
    /// <summary>
    /// Attempts to read next chunk length. Returns false if not enough bytes to read an int. Throws if an invalid number was read.
    /// </summary>
    /// <remarks>Note: Does not reset stream position.</remarks>
    /// <param name="chunkLength"></param>
    /// <returns></returns>
    private bool TryReadChunkLength(out int chunkLength)
    {
        Span<byte> buffer = stackalloc byte[4];
        int bytesRead = _stream.ReadAtLeast(buffer, 4, false);
        if (bytesRead < 4)
        {
            chunkLength = 0;
            return false;
        }

        chunkLength = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        if (chunkLength <= 0)
            throw new InvalidDataException("The read chunk length is less than or equal to zero.");
        if (chunkLength > _maxChunkLength)
            throw new InvalidDataException("The read chunk length is greater than the max chunk length.");

        return true;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            if (disposing)
            {
                if (!_leaveOpen)
                    _stream.Dispose();
            }
        }
    }

    // EVAL: Is DisposeAsync() actually implemented correctly? Does it matter enough for me to look further into it?
    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (!_leaveOpen)
                await _stream.DisposeAsync();

            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }

    #region Unsupported Overrides

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void SetLength(long value) => throw new NotSupportedException();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

    #endregion Unsupported Overrides
}