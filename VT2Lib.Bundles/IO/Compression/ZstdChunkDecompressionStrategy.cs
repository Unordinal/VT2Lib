using ZstdSharp;

namespace VT2Lib.Bundles.IO.Compression;

internal class ZstdChunkDecompressionStrategy : IChunkDecompressionStrategy
{
    private readonly Decompressor _decompressor;

    public ZstdChunkDecompressionStrategy()
    {
        _decompressor = new Decompressor();
    }

    public ZstdChunkDecompressionStrategy(ReadOnlySpan<byte> dictionary)
    {
        _decompressor = new Decompressor();
        _decompressor.LoadDictionary(dictionary);
    }

    public int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        return _decompressor.Unwrap(source, destination);
    }

    public void Dispose()
    {
        _decompressor.Dispose();
    }
}