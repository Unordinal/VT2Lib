namespace VT2Lib.Bundles.IO.Compression;

internal class ZlibChunkDecompressor : IChunkDecompressor
{
    public int UncompressedChunkLength => 0x10000;

    public ZlibChunkDecompressor()
    {
    }

    public int GetMaxDecompressedSize()
    {
        return UncompressedChunkLength;
    }

    public int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        return ZlibUtil.Decompress(source, destination);
    }

    public ValueTask<int> DecompressAsync(ReadOnlyMemory<byte> source, Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        return ZlibUtil.DecompressAsync(source, destination, cancellationToken);
    }

    public void Dispose()
    {
    }
}