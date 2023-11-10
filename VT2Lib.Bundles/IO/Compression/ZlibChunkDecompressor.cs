namespace VT2Lib.Bundles.IO.Compression;

internal class ZlibChunkDecompressor : IChunkDecompressor
{
    public ZlibChunkDecompressor()
    {
    }

    public int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        return ZlibUtil.Decompress(source, destination);
    }

    public void Dispose()
    {
    }
}