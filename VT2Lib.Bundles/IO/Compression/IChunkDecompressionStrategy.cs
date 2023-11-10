namespace VT2Lib.Bundles.IO.Compression;

internal interface IChunkDecompressionStrategy : IDisposable
{
    int Decompress(ReadOnlySpan<byte> source, Span<byte> destination);
}