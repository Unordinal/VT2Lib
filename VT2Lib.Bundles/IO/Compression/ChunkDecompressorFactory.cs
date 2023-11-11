namespace VT2Lib.Bundles.IO.Compression;

internal static class ChunkDecompressorFactory
{
    private const BundleVersion MinVersion = (BundleVersion)0xF000_0000;

    public static IChunkDecompressor Create(BundleVersion version, ReadOnlySpan<byte> compressionDictionary)
    {
        if (VersionUsesZstd(version))
        {
            if (compressionDictionary.IsEmpty)
                throw new ArgumentException($"Must provide a Zstd compression dictionary for bundle versions >= '{version}'");

            return new ZstdChunkDecompressor(GetVersionUncompressedChunkLength(version), compressionDictionary);
        }

        if (VersionUsesZlib(version))
            return new ZlibChunkDecompressor();

        throw new ArgumentOutOfRangeException(nameof(version), $"Unsupported bundle version '{(int)version:X8}'");
    }

    public static bool VersionUsesZlib(BundleVersion version)
    {
        return version is >= MinVersion and < BundleVersion.VT2XZtd;
    }

    public static bool VersionUsesZstd(BundleVersion version)
    {
        return version >= BundleVersion.VT2XZtd;
    }

    // For possible forward compatibility. (A dev discussed possibly changing the chunk length in the future.)
    private static int GetVersionUncompressedChunkLength(BundleVersion version)
    {
        return version switch
        {
            BundleVersion.VT2XZtd => 0x10000,
            >= MinVersion => 0x10000,
            _ => throw new ArgumentOutOfRangeException(nameof(version))
        };
    }
}