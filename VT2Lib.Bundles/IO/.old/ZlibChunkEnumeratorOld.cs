using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace VT2Lib.Bundles.IO;

internal sealed class ZlibChunkEnumeratorOld : IEnumerator<ZlibChunkEnumeratorOld.ZlibChunk>
{
    #region Public

    /// <summary>
    /// Gets the current chunk, fully reading and uncompressing it.
    /// </summary>
    public ZlibChunk Current => GetCurrent(true);

    object IEnumerator.Current => Current;

    private readonly ISeekableDataSource<byte> _chunkBytes;

    public ZlibChunkEnumeratorOld(Stream zlibChunks)
    {
        ArgumentNullException.ThrowIfNull(zlibChunks);
        if (!zlibChunks.CanSeek)
            throw new ArgumentException("The stream does not support seeking.", nameof(zlibChunks));

        _stream = zlibChunks;
    }

    public ZlibChunkEnumeratorOld(Memory<byte> zlibChunks)
    {
        if (zlibChunks.Length < 8)
            throw new ArgumentException("Memory does not have a valid zlib chunk.");

        _buffer = zlibChunks;
    }

    public ZlibChunkEnumeratorOld(ISeekableDataSource<byte> chunkBytes)
    {
        ArgumentNullException.ThrowIfNull(chunkBytes);
        _chunkBytes = chunkBytes;
    }

    public bool MoveNext()
    {
        ThrowIfDisposed();
        // Skip current chunk bytes if we didn't read them.
        if (_current is not null && _current.Data is null)
            Position += _current.Length;

        if (!TryReadLengthAndZlibHeader(out int length, out int zlibHeader))
        {
            _current = null;
            return false;
        }

        long startOffset = Position - 4;
        _current = new ZlibChunk(startOffset, length, null);
        return true;
    }

    /// <summary>
    ///     Gets the current chunk.
    /// </summary>
    /// <param name="fullRead">
    ///     If <see langword="true"/>, will read and uncompress the chunk's data;
    ///     otherwise, the returned <see cref="ZlibChunk"/> will contain only the chunk's metadata, such as its offset and compressed length.
    ///     <para/>
    ///     Can be called first with passing <see langword="false"/> as the argument before calling this method again with <see langword="true"/>
    ///     if the metadata is needed before fully reading the chunk.
    /// </param>
    /// <returns></returns>
    public ZlibChunk GetCurrent(bool fullRead)
    {
        ThrowIfDisposed();
        if (_current is null)
            throw new InvalidOperationException("'Current' is null; use MoveNext() first.");

        if (fullRead && _current.Data is null)
        {
            int length = _current.Length;
            byte[] chunkData = new byte[length];
            ReadCurrentFullChunk(chunkData);

            _current.Data = chunkData;
        }

        return _current;
    }

    /// <summary>
    /// Presumes position within stream or buffer is right after the length.
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    private void ReadCurrentFullChunk(Span<byte> destination)
    {
        Debug.Assert(_current is not null && _current.Data is null);
        if (_stream is not null)
        {
            if (_current.IsCompressed)
            {
                using var zlibStream = new ZLibStream(_stream, CompressionMode.Decompress, true);
                int bytesDecompressed = zlibStream.ReadAtLeast(destination, destination.Length, false);
            }
            else
            {
                int bytesRead = _stream.ReadAtLeast(destination, destination.Length, false);
                Debug.Assert(bytesRead == _current.Length);
            }
        }
        else
        {
            int bytesRead;
            if (_current.IsCompressed)
            {
                int bytesDecompressed = ZLibUtil.Decompress(GetSlicedBuffer(_current.Length), destination);
                bytesRead = _current.Length;
            }
            else
            {
                GetSlicedBuffer(_current.Length).CopyTo(destination);
                bytesRead = _current.Length;
            }

            _bufPos += bytesRead;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public sealed class ZlibChunk
    {
        /// <summary>
        /// Gets the start offset of the chunk within the data.
        /// </summary>
        public long StartOffset { get; }

        /// <summary>
        /// Gets the compressed length of the chunk.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets whether the chunk is compressed.
        /// </summary>
        public bool IsCompressed => Length == ZLibUtil.MaxChunkLength;

        /// <summary>
        /// Gets the uncompressed data contained within the chunk.
        /// </summary>
        public byte[]? Data { get; internal set; }

        internal ZlibChunk(long startOffset, int length, byte[]? data)
        {
            Debug.Assert(startOffset >= 0);
            Debug.Assert(length >= 0);
            Debug.Assert(data is null || data.Length == length);

            StartOffset = startOffset;
            Length = length;
            Data = data;
        }
    }

    #endregion Public

    #region Private

    private const int SizeOfLenAndHeader = 8;

    private long Position
    {
        get
        {
            if (_stream is not null)
                return _stream.Position;

            Debug.Assert(_buffer.HasValue);
            return _bufPos;
        }
        set
        {
            if (_stream is not null)
                _stream.Position = value;

            Debug.Assert(_buffer.HasValue);
            _bufPos = (int)value;
        }
    }

    private readonly Stream? _stream;
    private readonly Memory<byte>? _buffer;

    private int _bufPos;
    private ZlibChunk? _current;
    private bool _disposed;

    [MemberNotNull(nameof(_buffer))]
    private Span<byte> GetSlicedBuffer()
    {
        Debug.Assert(_buffer.HasValue);
        return _buffer.Value.Span[_bufPos..];
    }

    [MemberNotNull(nameof(_buffer))]
    private Span<byte> GetSlicedBuffer(int length)
    {
        Debug.Assert(_buffer.HasValue);
        Debug.Assert(length >= 0);

        int maxLen = Math.Min(_buffer.Value.Length - _bufPos, length);
        return _buffer.Value.Span.Slice(_bufPos, maxLen);
    }

    private int RawRead(Span<byte> destination)
    {
        int bytesRead;
        if (_stream is not null)
        {
            bytesRead = _stream.Read(destination);
        }
        else
        {
            Span<byte> buffer = GetSlicedBuffer(destination.Length);
            buffer.CopyTo(destination);

            bytesRead = buffer.Length;
            _bufPos += bytesRead;
        }

        return bytesRead;
    }

    private bool TryReadLengthAndZlibHeader(out int length, out int zlibHeader)
    {
        Span<byte> lenAndHeader = stackalloc byte[SizeOfLenAndHeader];
        int bytesRead = RawRead(lenAndHeader);
        Position -= 4; // revert to before Zlib header.

        if (bytesRead < SizeOfLenAndHeader)
        {
            length = zlibHeader = 0;
            return false;
        }

        length = BinaryPrimitives.ReadInt32LittleEndian(lenAndHeader);
        zlibHeader = BinaryPrimitives.ReadInt32LittleEndian(lenAndHeader[4..]);
        bool isCompressed = length < ZLibUtil.MaxChunkLength;

        if (length is < 0 or > ZLibUtil.MaxChunkLength)
            return false;
        if (isCompressed && zlibHeader != ZLibUtil.ChunkHeader)
            return false;

        return true;
    }

    private bool BufferRangeIsValid(int length)
    {
        Debug.Assert(_buffer.HasValue);
        return ((ulong)(uint)_bufPos + (ulong)(uint)length <= (ulong)(uint)_buffer.Value.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerHidden, DebuggerStepThrough]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    #endregion Private
}