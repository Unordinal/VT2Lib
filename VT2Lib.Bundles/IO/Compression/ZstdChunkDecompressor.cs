using ZstdSharp;

namespace VT2Lib.Bundles.IO.Compression;

internal class ZstdChunkDecompressor : IChunkDecompressor
{
    public int UncompressedChunkLength { get; }

    private readonly Decompressor _decompressor;

    public ZstdChunkDecompressor(int uncompressedChunkLength)
    {
        UncompressedChunkLength = uncompressedChunkLength;
        _decompressor = new Decompressor();
    }

    public ZstdChunkDecompressor(int uncompressedChunkLength, ReadOnlySpan<byte> dictionary)
    {
        UncompressedChunkLength = uncompressedChunkLength;
        _decompressor = new Decompressor();
        _decompressor.LoadDictionary(dictionary);
    }

    public int GetMaxDecompressedSize()
    {
        //return Decompressor.GetDecompressedSize(...);
        return UncompressedChunkLength * 2;
    }

    public int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        return _decompressor.Unwrap(source, destination);
    }

    public unsafe ValueTask<int> DecompressAsync(ReadOnlyMemory<byte> source, Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        fixed (byte* pSource = source.Span)
        {
            using var memStream = new UnmanagedMemoryStream(pSource, source.Length);
            using var decompStream = new DecompressionStream(memStream, _decompressor);

            return decompStream.ReadAtLeastAsync(destination, destination.Length, false, cancellationToken);
        }
    }

    public void Dispose()
    {
        _decompressor.Dispose();
    }
}