namespace VT2Lib.Bundles.IO.Compression;

internal interface ICompressedChunkReader : IDisposable
{
    int GetChunkCount();

    long SeekToChunk(int chunkIndex);

    int ReadChunk(Span<byte> destination, bool decompress);

    int ReadChunk(int chunkIndex, Span<byte> destination, bool decompress);
}