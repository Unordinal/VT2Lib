using System.Buffers.Binary;
using System.Diagnostics;
using VT2Lib.Core.Collections;
using VT2Lib.Core.Extensions;
using ZstdSharp;

namespace VT2Lib.Bundles.IO.Compression;

internal sealed class ZLibChunkReader : CompressedChunkReader
{
    public ZLibChunkReader(Stream stream, bool leaveOpen) : base(stream, leaveOpen)
    {
    }

    /// <summary>
    /// Gets the number of chunks in the stream.
    /// </summary>
    /// <returns>The number of chunks in the stream.</returns>
    public override int GetChunkCount()
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

    public override long SeekToChunk(int chunkIndex)
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

    public override int ReadChunk(
        Span<byte> destination,
        bool decompress = true)
    {
        ThrowIfDisposed();
        if (destination.Length < ZLibUtil.MaxChunkLength)
            throw new ArgumentException("Destination span not large enough.", nameof(destination));
        if (!TryReadChunkSize(out int chunkSize))
            return 0;

        bool isCompressed = chunkSize < ZLibUtil.MaxChunkLength;
        if (isCompressed && decompress)
        {
            using RentedArray<byte> buffer = new(chunkSize);
            _stream.ReadExactly(buffer.Span);
            ZLibUtil.Decompress(buffer.Span, destination);
        }
        else
        {
            _stream.ReadExactly(destination[..chunkSize]);
        }

        return chunkSize;
    }

    public override int ReadChunk(
        int chunkIndex,
        Span<byte> destination,
        bool decompress = true)
    {
        SeekToChunk(chunkIndex);
        return ReadChunk(destination, decompress);
    }

    public async Task<int> ReadChunkAsync(
        Memory<byte> destination,
        bool decompress = true,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (destination.Length < ZLibUtil.MaxChunkLength)
            throw new ArgumentException("Destination span not large enough.", nameof(destination));
        if (!TryReadChunkSize(out int chunkSize))
            return 0;

        cancellationToken.ThrowIfCancellationRequested();
        bool isCompressed = chunkSize < ZLibUtil.MaxChunkLength;
        if (isCompressed && decompress)
        {
            using RentedArray<byte> buffer = new(chunkSize);
            await _stream.ReadExactlyAsync(buffer.Memory, cancellationToken)
                .ConfigureAwait(false);
            await ZLibUtil.DecompressAsync(buffer.Memory, destination, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await _stream.ReadExactlyAsync(destination[..chunkSize], cancellationToken)
                .ConfigureAwait(false);
        }

        return chunkSize;
    }

    protected override bool TryReadChunkSize(out int chunkSize)
    {
        Debug.Assert(_disposed == false);

        Span<byte> chunkSizeBytes = stackalloc byte[4];
        int bytesRead = _stream.ReadAtLeast(chunkSizeBytes, chunkSizeBytes.Length, false);

        chunkSize = 0;
        if (bytesRead < chunkSizeBytes.Length)
            return false;

        chunkSize = BinaryPrimitives.ReadInt32LittleEndian(chunkSizeBytes);
        Debug.Assert(chunkSize is > 0 and <= ZLibUtil.MaxChunkLength);
        return true;
    }

    private bool IndexIsInOffsetsList(int chunkIndex)
    {
        return chunkIndex < _chunkOffsets.Count;
    }

    private bool CurrChunkExists()
    {
        bool exists = TryReadChunkSize(out _);
        _stream.Position -= 4;
        return exists;
    }

    [DebuggerHidden, DebuggerStepThrough]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}