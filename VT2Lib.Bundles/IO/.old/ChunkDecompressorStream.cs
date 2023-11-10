using System.Buffers.Binary;
using System.ComponentModel;
using VT2Lib.Core.Extensions;

namespace VT2Lib.Bundles.IO;

internal sealed class ChunkDecompressorStream : Stream
{
    private readonly Stream _stream;
    private readonly int _numChunksToBuffer;
    private bool _disposed;

    private readonly long _chunksStartOffset;
    private readonly List<long> _chunkOffsets;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Readability.")]
    public ChunkDecompressorStream(Stream stream, int numChunksToBuffer = 2, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
            throw new ArgumentException("The stream does not support seeking.", nameof(stream));

        _stream = leaveOpen ? new LeaveOpenStream(stream) : stream;
        _numChunksToBuffer = numChunksToBuffer;

        _chunksStartOffset = _stream.Position;
        _chunkOffsets = new List<long>();
        _chunkOffsets.Add(_chunksStartOffset);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;
        _stream.Dispose();
    }

    private bool TryReadChunkSize(out int chunkSize, bool resetPosition = false)
    {
        long origPos = resetPosition ? _stream.Position : 0;
        Span<byte> buffer = stackalloc byte[4];

        int bytesRead = _stream.ReadAtLeast(buffer, buffer.Length, false);
        if (resetPosition)
            _stream.Position = origPos;

        chunkSize = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        return bytesRead == buffer.Length;
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
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Throws <see cref="NotSupportedException"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        throw new NotSupportedException();
    }

    #endregion Unsupported Members
}