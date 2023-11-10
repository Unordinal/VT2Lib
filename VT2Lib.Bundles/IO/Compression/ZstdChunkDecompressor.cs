using ZstdSharp;

namespace VT2Lib.Bundles.IO.Compression;

internal class ZstdChunkDecompressor : IChunkDecompressor
{
    private readonly Decompressor _decompressor;

    public ZstdChunkDecompressor()
    {
        _decompressor = new Decompressor();
    }

    public ZstdChunkDecompressor(ReadOnlySpan<byte> dictionary)
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