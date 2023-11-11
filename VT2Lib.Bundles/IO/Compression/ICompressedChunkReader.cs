namespace VT2Lib.Bundles.IO.Compression;

public interface ICompressedChunkReader : IDisposable
{
    int UncompressedChunkLength { get; }

    int GetChunkCount();

    int GetChunkMaxDecompressedSize();

    long SeekToChunk(int chunkIndex);

    int ReadChunk(Span<byte> destination, bool decompress = true);

    Task<int> ReadChunkAsync(Memory<byte> destination, bool decompress = true, CancellationToken cancellationToken = default);
}