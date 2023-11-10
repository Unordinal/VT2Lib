using System.Diagnostics;

namespace VT2Lib.Bundles.IO.Compression;

internal sealed class ZLibChunk
{
    /// <summary>
    /// Gets the start offset of the chunk within the data.
    /// </summary>
    public long StartOffset { get; }

    /// <summary>
    /// Gets the compressed length of the chunk.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets whether the chunk is compressed.
    /// </summary>
    public bool IsCompressed => Length == ZLibUtil.MaxChunkLength;

    /// <summary>
    /// Gets the uncompressed data contained within the chunk.
    /// </summary>
    public byte[]? Data { get; internal set; }

    internal ZLibChunk(long startOffset, int length, byte[]? data)
    {
        Debug.Assert(startOffset >= 0);
        Debug.Assert(length >= 0);
        Debug.Assert(data is null || data.Length == length);

        StartOffset = startOffset;
        Length = length;
        Data = data;
    }
}