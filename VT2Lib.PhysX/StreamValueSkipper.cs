namespace VT2Lib.PhysX;

internal class StreamValueSkipper
{
    private readonly int _bytesToSkip;

    public StreamValueSkipper(int bytesToSkip)
    {
        _bytesToSkip = bytesToSkip;
    }

    public virtual void Skip(Stream stream)
    {
        stream.Position += _bytesToSkip;
    }
}