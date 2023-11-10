namespace VT2Lib.Bundles.IO.Compression;

public interface ICompressedChunkReader : IDisposable
{
    int GetChunkCount();

    long SeekToChunk(int chunkIndex);

    int ReadChunk(Span<byte> destination, bool decompress = true);

    int ReadChunk(int chunkIndex, Span<byte> destination, bool decompress = true);
}