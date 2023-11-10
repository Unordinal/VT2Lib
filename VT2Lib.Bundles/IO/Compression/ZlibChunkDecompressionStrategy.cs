namespace VT2Lib.Bundles.IO.Compression;

internal class ZlibChunkDecompressionStrategy : IChunkDecompressionStrategy
{
    public ZlibChunkDecompressionStrategy()
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