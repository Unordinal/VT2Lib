using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace VT2Lib.Bundles.IO;

internal sealed class ZLibChunkReader : IDisposable
{
    private readonly Stream _chunkStream;
    private readonly bool _leaveOpen;

    private readonly List<long> _chunkOffsets = new();
    private readonly List<ZLibChunk> _chunks = new();
    private int? _chunkCount;
    private bool _disposed;

    public ZLibChunkReader(Stream chunkStream, bool leaveOpen)
    {
        ArgumentNullException.ThrowIfNull(chunkStream);
        if (!chunkStream.CanSeek)
            throw new ArgumentException("The stream is not seekable.");

        _chunkStream = chunkStream;
        _leaveOpen = leaveOpen;
        _chunkOffsets.Add(_chunkStream.Position);
    }

    public int GetChunkCount()
    {
        if (_chunkCount.HasValue)
            return _chunkCount.Value;

        ReadAllChunks(false);
        return _chunkCount.Value;
    }

    public ZLibChunk GetChunk(int index, bool fullRead)
    {
        return EnumerateChunks(index, fullRead).First();
    }

    public IEnumerable<ZLibChunk> EnumerateChunks(int startingChunk, bool fullRead)
    {
        if (startingChunk < 0)
            throw new ArgumentOutOfRangeException(nameof(startingChunk));

        int index = Math.Min(startingChunk, _chunkOffsets.Count - 1);
        _chunkStream.Position = _chunkOffsets[index];

        using var enumerator = new Enumerator(this);
        while (enumerator.MoveNext())
        {
            bool currIndexNotBeforeStarting = index >= startingChunk;
            ZLibChunk chunk = enumerator.GetCurrent(fullRead && currIndexNotBeforeStarting);

            if (index == _chunkOffsets.Count)
                _chunkOffsets.Add(_chunkStream.Position);

            if (index == _chunks.Count)
                _chunks.Add(chunk);
            else if (fullRead && _chunks[index].Data is null)
                _chunks[index].Data = chunk.Data;

            if (currIndexNotBeforeStarting)
                yield return chunk;

            index++;
        }

        if (startingChunk >= index)
            throw new ArgumentOutOfRangeException(nameof(startingChunk));

        _chunkCount = index;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (!_leaveOpen)
            _chunkStream.Dispose();

        _disposed = true;
    }

    [MemberNotNull(nameof(_chunkCount))]
    private void ReadAllChunks(bool fullRead)
    {
        // This feels dirty.
        foreach (var _ in EnumerateChunks(0, fullRead)) ;
        Debug.Assert(_chunkCount.HasValue);
    }

    private struct Enumerator : IEnumerator<ZLibChunk>
    {
        readonly ZLibChunk IEnumerator<ZLibChunk>.Current => GetCurrent(true);

        readonly object IEnumerator.Current => ((IEnumerator<ZLibChunk>)this).Current;

        private readonly ZLibChunkReader _reader;

        private ZLibChunk? _current;

        internal Enumerator(ZLibChunkReader reader)
        {
            ArgumentNullException.ThrowIfNull(reader);
            _reader = reader;
        }

        public bool MoveNext()
        {
            ThrowIfDisposed();
            if (_current is not null && _current.Data is null)
                _reader._chunkStream.Position += _current.Length + 4; // Position is before the bytes specifying the chunk's length, so +4.

            if (!TryReadLengthAndHeader(out int length, out int zlibHeader))
            {
                _current = null;
                return false;
            }

            _current = new ZLibChunk(_reader._chunkStream.Position, length, null);
            return true;
        }

        public readonly ZLibChunk GetCurrent(bool fullRead)
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

        public readonly void Dispose()
        {
        }

        private readonly void ReadCurrentChunkFully()
        {
            Debug.Assert(_current is not null);
            Debug.Assert(_current.Data is null);

            // Position will be before the chunk's length, so we need to move it forward.
            _reader._chunkStream.Position += 4;

            MemoryStream dataStream = new(ZLibUtil.MaxChunkLength);
            if (_current.IsCompressed)
            {
                using var zlibStream = new ZLibStream(_reader._chunkStream, CompressionMode.Decompress, true);
                zlibStream.CopyTo(dataStream);
            }
            else
            {
                _reader._chunkStream.CopyTo(dataStream);
            }

            // EVAL: Should we set position? The answer depends on whether the ZLibStream reads the last chunk with the zero-padding.
            // I assume it does, so it's not necessary. I suppose we possibly could pre-allocate a byte[] to ZLibUtil.MaxChunkLength and just
            // read directly into that but I don't think it's needed unless we want to squeeze a bit more perf.
            _current.Data = dataStream.ToArray();
        }

        private readonly bool TryReadLengthAndHeader(out int length, out int zlibHeader)
        {
            Debug.Assert(!_reader._disposed);
            const int SizeOfLenAndHeader = 8;
            Span<byte> lenAndHeader = stackalloc byte[SizeOfLenAndHeader];

            length = 0;
            zlibHeader = 0;

            long origPos = _reader._chunkStream.Position;
            int bytesRead = _reader._chunkStream.ReadAtLeast(lenAndHeader, SizeOfLenAndHeader, false);
            _reader._chunkStream.Position = origPos;

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
        private readonly void ThrowIfDisposed()
        {
            if (_reader._disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}