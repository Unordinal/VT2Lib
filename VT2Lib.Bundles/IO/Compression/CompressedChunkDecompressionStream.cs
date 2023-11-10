using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using VT2Lib.Core.Collections;

namespace VT2Lib.Bundles.IO.Compression;

internal sealed class CompressedChunkDecompressionStream : Stream
{
    public override bool CanRead => _disposed == false;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    private readonly ICompressedChunkReader _chunkReader;
    private readonly bool _leaveOpen;
    private bool _disposed;

    private readonly RentedArray<byte> _buffer;
    private readonly int _numChunksToBuffer;
    private int _readPos;
    private int _readLen;

    public CompressedChunkDecompressionStream(ICompressedChunkReader chunkReader, int numChunksToBuffer = 2)
    {
        ArgumentNullException.ThrowIfNull(chunkReader);
        if (numChunksToBuffer <= 0)
            throw new ArgumentOutOfRangeException(nameof(numChunksToBuffer));

        _chunkReader = chunkReader;

        _buffer = new RentedArray<byte>(ZlibUtil.MaxChunkLength * numChunksToBuffer);
        _numChunksToBuffer = numChunksToBuffer;
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

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ValidateBufferArguments(buffer, offset, count);
        return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    /*public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        int bytesFromBuffer = ReadFromBuffer(buffer.Span);
        if (bytesFromBuffer == buffer.Length)
            return bytesFromBuffer;

        int bytesFromNewChunks = await FillBufferAsync(cancellationToken)
            .ConfigureAwait(false);
        if (bytesFromNewChunks == 0)
            return bytesFromBuffer;

        return ReadFromBuffer(buffer.Span[bytesFromBuffer..]) + bytesFromBuffer;
    }*/

    public override void Flush()
    {
        _readPos = 0;
        _readLen = 0;
    }

    private int ReadFromBuffer(Span<byte> destination)
    {
        int bytesToCopy = Math.Min(_readLen - _readPos, destination.Length);
        Debug.Assert(bytesToCopy >= 0);

        if (bytesToCopy > 0)
        {
            _buffer.AsSpan(_readPos, bytesToCopy).CopyTo(destination);
            _readPos += bytesToCopy;
        }

        return bytesToCopy;
    }

    private int FillBuffer()
    {
        Debug.Assert(_disposed == false);
        Debug.Assert(_readPos == _readLen);

        int totalBytesRead = 0;
        for (int i = 0; i < _numChunksToBuffer; i++)
        {
            int bytesRead = _chunkReader.ReadChunk(_buffer.AsSpan(totalBytesRead));
            if (bytesRead == 0)
                break;

            totalBytesRead += ZlibUtil.MaxChunkLength;
        }

        _readPos = 0;
        _readLen = totalBytesRead;
        return totalBytesRead;
    }

    /*private async ValueTask<int> FillBufferAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(_disposed == false);
        Debug.Assert(_readPos == _readLen);
        cancellationToken.ThrowIfCancellationRequested();

        int totalBytesRead = 0;
        for (int i = 0; i < _numChunksToBuffer; i++)
        {
            int bytesRead = await _chunkReader.ReadChunkAsync(_buffer.AsMemory(totalBytesRead), true, cancellationToken)
                .ConfigureAwait(false);
            if (bytesRead == 0)
                break;

            totalBytesRead += bytesRead;
        }

        _readPos = 0;
        _readLen = totalBytesRead;
        return totalBytesRead;
    }*/

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;
        if (!disposing)
            return;

        _chunkReader.Dispose();
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    #region Unsupported Members

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override long Length => throw new NotSupportedException();

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

    #endregion Unsupported Members
}