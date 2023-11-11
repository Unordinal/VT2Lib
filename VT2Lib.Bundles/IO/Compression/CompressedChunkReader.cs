using System.Buffers.Binary;
using System.Diagnostics;
using VT2Lib.Core.Collections;
using VT2Lib.Core.Extensions;

namespace VT2Lib.Bundles.IO.Compression;

internal class CompressedChunkReader : ICompressedChunkReader
{
    public int UncompressedChunkLength => _decompressor.UncompressedChunkLength;

    protected readonly Stream _stream;
    protected readonly bool _leaveOpen;
    protected readonly IChunkDecompressor _decompressor;

    protected int? _chunkCount;
    protected readonly List<long> _chunkOffsets = new();
    protected bool _disposed;

    public CompressedChunkReader(Stream stream, bool leaveOpen, IChunkDecompressor decompressor)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(decompressor);
        if (!stream.CanSeek)
            throw new ArgumentException("The stream is not seekable.", nameof(stream));

        _stream = stream;
        _leaveOpen = leaveOpen;
        _decompressor = decompressor;

        _chunkOffsets.Add(stream.Position);
    }

    public virtual int GetChunkCount()
    {
        ThrowIfDisposed();
        if (_chunkCount.HasValue)
            return _chunkCount.Value;

        //Debug.Assert(CurrentChunk < _chunkOffsets.Count);
        long origPos = _stream.Position;

        _stream.Position = _chunkOffsets.Last();
        int currChunk = _chunkOffsets.Count - 1;

        while (TryReadChunkSize(out int chunkSize))
        {
            _stream.Skip(chunkSize);
            currChunk++;
            if (CurrChunkExists() && currChunk >= _chunkOffsets.Count)
                _chunkOffsets.Add(_stream.Position);
        }

        _stream.Position = origPos;
        _chunkCount = currChunk;
        return currChunk;
    }

    public virtual int GetChunkMaxDecompressedSize()
    {
        ThrowIfDisposed();
        /*if (!TryReadChunkSize(out int chunkSize))
            return 0;*/

        return _decompressor.GetMaxDecompressedSize();
    }

    public virtual long GetChunkMaxDecompressedSize(int chunkIndex)
    {
        SeekToChunk(chunkIndex);
        return GetChunkMaxDecompressedSize();
    }

    public virtual long SeekToChunk(int chunkIndex)
    {
        ThrowIfDisposed();
        if (chunkIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(chunkIndex));
        if (_chunkCount.HasValue && chunkIndex >= _chunkCount.Value)
            throw new ArgumentOutOfRangeException(nameof(chunkIndex));

        /*if (CurrentChunk == chunkIndex)
            return _stream.Position;*/

        if (IndexIsInOffsetsList(chunkIndex))
        {
            //CurrentChunk = chunkIndex;
            return _stream.Position = _chunkOffsets[chunkIndex];
        }

        // not very DRY of you
        _stream.Position = _chunkOffsets.Last();
        int currChunk = _chunkOffsets.Count - 1;
        while (currChunk < chunkIndex && TryReadChunkSize(out int chunkSize))
        {
            _stream.Skip(chunkSize);
            currChunk++;
            if (CurrChunkExists() && currChunk >= _chunkOffsets.Count)
                _chunkOffsets.Add(_stream.Position);
        }

        //CurrentChunk = currChunk;
        return _stream.Position;
    }

    public virtual int ReadChunk(
        Span<byte> destination,
        bool decompress = true)
    {
        ThrowIfDisposed();
        if (destination.Length < GetChunkMaxDecompressedSize())
            throw new ArgumentException("Destination span not large enough.", nameof(destination));
        if (!TryReadChunkSize(out int chunkSize))
            return 0;

        int bytesRead = 0;
        bool isCompressed = chunkSize != _decompressor.UncompressedChunkLength;
        if (isCompressed && decompress)
        {
            using RentedArray<byte> buffer = new(chunkSize);
            _stream.ReadExactly(buffer.Span);
            bytesRead = _decompressor.Decompress(buffer.Span, destination);
        }
        else
        {
            Debug.Assert(chunkSize ==  _decompressor.UncompressedChunkLength);
            _stream.ReadExactly(destination[..chunkSize]);
            bytesRead = chunkSize;
        }

        return bytesRead;
    }

    public virtual int ReadChunk(
        int chunkIndex,
        Span<byte> destination,
        bool decompress = true)
    {
        SeekToChunk(chunkIndex);
        return ReadChunk(destination, decompress);
    }

    public virtual async Task<int> ReadChunkAsync(
        Memory<byte> destination,
        bool decompress = true,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (destination.Length < GetChunkMaxDecompressedSize())
            throw new ArgumentException("Destination span not large enough.", nameof(destination));
        if (!TryReadChunkSize(out int chunkSize))
            return 0;

        cancellationToken.ThrowIfCancellationRequested();

        int bytesRead = 0;
        bool isCompressed = chunkSize != _decompressor.UncompressedChunkLength;
        if (isCompressed && decompress)
        {
            using RentedArray<byte> buffer = new(chunkSize);
            await _stream.ReadExactlyAsync(buffer.Memory, cancellationToken)
                .ConfigureAwait(false);

            bytesRead = await _decompressor.DecompressAsync(buffer.Memory, destination, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            Debug.Assert(chunkSize == _decompressor.UncompressedChunkLength);

            await _stream.ReadExactlyAsync(destination[..chunkSize], cancellationToken)
                .ConfigureAwait(false);
            bytesRead = chunkSize;
        }

        return bytesRead;
    }

    public virtual Task<int> ReadChunkAsync(
        int chunkIndex,
        Memory<byte> destination,
        bool decompress = true,
        CancellationToken cancellationToken = default)
    {
        SeekToChunk(chunkIndex);
        return ReadChunkAsync(destination, decompress, cancellationToken);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual bool TryReadChunkSize(out int chunkSize)
    {
        Debug.Assert(_disposed == false);

        Span<byte> chunkSizeBytes = stackalloc byte[4];
        int bytesRead = _stream.ReadAtLeast(chunkSizeBytes, chunkSizeBytes.Length, false);

        chunkSize = 0;
        if (bytesRead < chunkSizeBytes.Length)
            return false;

        chunkSize = BinaryPrimitives.ReadInt32LittleEndian(chunkSizeBytes);
        if (chunkSize < 0)
            throw new InvalidDataException($"Bad chunk size for compressed chunk at position {_stream.Position}. ({chunkSize} < 0)");

        return true;
    }

    protected bool IndexIsInOffsetsList(int chunkIndex)
    {
        return chunkIndex < _chunkOffsets.Count;
    }

    protected virtual bool CurrChunkExists()
    {
        bool exists = TryReadChunkSize(out _);
        _stream.Position -= 4;
        return exists;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (!_leaveOpen)
                _stream.Dispose();
        }

        _disposed = true;
    }

    ~CompressedChunkReader()
    {
        Dispose(disposing: false);
    }

    [DebuggerHidden, DebuggerStepThrough]
    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}