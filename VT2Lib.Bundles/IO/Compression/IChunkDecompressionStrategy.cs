namespace VT2Lib.Bundles.IO.Compression;

public interface IChunkDecompressionStrategy : IDisposable
{
    int Decompress(ReadOnlySpan<byte> source, Span<byte> destination);
}