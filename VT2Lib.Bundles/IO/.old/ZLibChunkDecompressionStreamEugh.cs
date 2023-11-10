using System.ComponentModel;
using System.Diagnostics;
using VT2Lib.Core.Collections;

namespace VT2Lib.Bundles.IO;

internal class ZLibChunkDecompressionStream : Stream
{
    public override bool CanRead => _disposed == false;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public Stream BaseStream => _baseStream;

    private readonly Stream _baseStream;

    private readonly RentedArray<byte> _buffer;
    private int _readPos;
    private int _readLen;

    private bool _disposed;

    public ZLibChunkDecompressionStream(Stream baseStream)
    {
        ArgumentNullException.ThrowIfNull(baseStream);
        _baseStream = baseStream;
        _buffer = new RentedArray<byte>(ZLibUtil.MaxChunkLength);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        ValidateBufferArguments(buffer, offset, count);
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        return base.Read(buffer);
    }

    public override void Flush()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _baseStream.Dispose();
            _buffer.Dispose();
        }

        _disposed = true;
    }

    [DebuggerHidden, DebuggerStepThrough]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    #region Unsupported Members

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override long Length => throw new NotSupportedException();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    #endregion Unsupported Members
}