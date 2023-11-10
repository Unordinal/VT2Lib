using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics;
using System.IO.Compression;

namespace VT2Lib.Bundles.IO;

/**
 * ZLib chunk layout:
 * 
 * | 4 bytes |      {length} bytes      |
 * |=========|==========================|
 * | length  | zlib header | chunk data |
 * 
 */
internal sealed partial class ZLibChunkEnumerator : IEnumerator<ZLibChunk>
{
    public ZLibChunk Current => GetCurrent(true);

    object IEnumerator.Current => Current;

    private readonly Stream _chunkStream;
    private readonly bool _leaveOpen;

    private ZLibChunk? _current;
    private bool _disposed;

    public ZLibChunkEnumerator(Stream chunkStream, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(chunkStream);
        if (!chunkStream.CanSeek)
            throw new ArgumentException("The stream is not seekable.", nameof(chunkStream));

        _chunkStream = chunkStream;
        _leaveOpen = leaveOpen;
    }

    public bool MoveNext()
    {
        ThrowIfDisposed();
        if (_current is not null && _current.Data is null)
            _chunkStream.Position += _current.Length + 4; // Position is before the bytes specifying the chunk's length, so +4.

        if (!TryReadLengthAndHeader(out int length, out int zlibHeader))
        {
            _current = null;
            return false;
        }

        _current = new ZLibChunk(_chunkStream.Position, length, null);
        return true;
    }

    public ZLibChunk GetCurrent(bool fullRead)
    {
        ThrowIfDisposed();
        if (_current is null)
            throw new InvalidOperationException("'Current' is null; use MoveNext() first.");

        if (fullRead && _current.Data is null)
            ReadCurrentChunkFully();

        return _current;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (!_leaveOpen)
            _chunkStream.Dispose();

        _disposed = true;
    }

    private void ReadCurrentChunkFully()
    {
        Debug.Assert(_current is not null);
        Debug.Assert(_current.Data is null);

        // Position will be before the chunk's length, so we need to move it forward.
        _chunkStream.Position += 4;

        MemoryStream dataStream = new(ZLibUtil.MaxChunkLength);
        if (_current.IsCompressed)
        {
            using var zlibStream = new ZLibStream(_chunkStream, CompressionMode.Decompress, true);
            zlibStream.CopyTo(dataStream);
        }
        else
        {
            _chunkStream.CopyTo(dataStream);
        }

        // EVAL: Should we set position? The answer depends on whether the ZLibStream reads the last chunk with the zero-padding.
        // I assume it does, so it's not necessary. I suppose we possibly could pre-allocate a byte[] to ZLibUtil.MaxChunkLength and just
        // read directly into that but I don't think it's needed unless we want to squeeze a bit more perf.
        _current.Data = dataStream.ToArray();
    }

    private bool TryReadLengthAndHeader(out int length, out int zlibHeader)
    {
        Debug.Assert(!_disposed);
        const int SizeOfLenAndHeader = 8;
        Span<byte> lenAndHeader = stackalloc byte[SizeOfLenAndHeader];

        length = 0;
        zlibHeader = 0;

        long origPos = _chunkStream.Position;
        int bytesRead = _chunkStream.ReadAtLeast(lenAndHeader, SizeOfLenAndHeader, false);
        _chunkStream.Position = origPos;

        if (bytesRead != SizeOfLenAndHeader)
            return false;

        length = BinaryPrimitives.ReadInt32LittleEndian(lenAndHeader);
        zlibHeader = BinaryPrimitives.ReadInt32LittleEndian(lenAndHeader[4..]);
        if (length is < 0 or > ZLibUtil.MaxChunkLength)
            return false;

        bool isCompressed = length == ZLibUtil.MaxChunkLength;
        if (isCompressed && zlibHeader != ZLibUtil.ChunkHeader)
            return false;

        return true;
    }

    [DebuggerHidden, DebuggerStepThrough]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}