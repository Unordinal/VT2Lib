namespace VT2Lib.Bundles.IO.Compression;

public interface IChunkDecompressor : IDisposable
{
    /// <summary>
    /// Gets the length that signifies that a chunk is uncompressed.
    /// </summary>
    int UncompressedChunkLength { get; }

    /// <summary>
    /// Gets the max size of a decompressed chunk.
    /// </summary>
    /// <returns></returns>
    int GetMaxDecompressedSize();

    /// <summary>
    /// Decompresses the chunk in <paramref name="source"/> into <paramref name="destination"/>.
    /// </summary>
    /// <param name="source">The chunk data.</param>
    /// <param name="destination">The destination to decompress the chunk into.</param>
    /// <returns>The number of bytes decompressed.</returns>
    int Decompress(ReadOnlySpan<byte> source, Span<byte> destination);

    /// <summary>
    /// Asynchronously decompresses the chunk in <paramref name="source"/> into <paramref name="destination"/>.
    /// </summary>
    /// <param name="source">The chunk data.</param>
    /// <param name="destination">The destination to decompress the chunk into.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <returns>A task representing the number of bytes decompressed.</returns>
    ValueTask<int> DecompressAsync(ReadOnlyMemory<byte> source, Memory<byte> destination, CancellationToken cancellationToken = default);
}