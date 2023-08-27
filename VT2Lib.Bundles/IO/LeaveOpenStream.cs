namespace VT2Lib.Bundles.IO;

internal sealed class LeaveOpenStream : Stream
{
    public Stream BaseStream => _stream;

    private readonly Stream _stream;

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position { get => _stream.Position; set => _stream.Position = value; }

    public LeaveOpenStream(Stream stream)
    {
        _stream = stream;
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
    }

    public override void Flush()
    {
        _stream.Flush();
    }
}