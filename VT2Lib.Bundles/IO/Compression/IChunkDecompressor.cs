namespace VT2Lib.Bundles.IO.Compression;

public interface IChunkDecompressor : IDisposable
{
    int Decompress(ReadOnlySpan<byte> source, Span<byte> destination);
}