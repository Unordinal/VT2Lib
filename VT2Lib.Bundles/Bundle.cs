﻿using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics;
using VT2Lib.Bundles.Extensions;
using VT2Lib.Bundles.IO;
using VT2Lib.Bundles.IO.Compression;
using VT2Lib.Bundles.Resources;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Extensions;

namespace VT2Lib.Bundles;

public sealed class Bundle : IDisposable
{
    private const int PropertySectionSize = PropertyCount * sizeof(ulong);
    private const int PropertyCount = 32; // TODO: Figure out what the heck bundle properties even are

    public static ImmutableArray<BundleVersion> SupportedVersions { get; } = new BundleVersion[]
    {
        //BundleVersion.VT2,
        BundleVersion.VT2X,
        BundleVersion.VT2XC // FIXME: TEMP
    }.ToImmutableArray();

    /// <summary>
    /// Gets the absolute path of the bundle's file name, if it was created via path.
    /// Returns <see langword="null"/> if this bundle was created via stream.
    /// </summary>
    public string? FileName { get; }

    public BundleVersion Version { get; }

    public long Size { get; }

    public bool IsCompressed { get; }

    public IReadOnlyList<IDString64> Properties => _properties.AsReadOnlyEx();

    public IReadOnlyList<BundledResourceMeta> ResourcesMeta => _resourcesMeta.AsReadOnlyEx();

    public IReadOnlyList<BundleResource> Resources { get; }

    /// <summary>
    /// Gets this bundle's associated asset .stream file, if it has one.
    /// </summary>
    public Stream? AssetStream { get; private set; }

    private readonly Stream? _assetStream;
    private readonly IIDString64Provider _idStringProvider;
    private readonly IDString64[] _properties;
    private readonly BundledResourceMeta[] _resourcesMeta;
    private readonly BundledResource[] _resources;

    private bool _disposed;

    private Bundle(Stream? assetStream, BundleHeader bundleHeader, BundledResource[] resources, IIDString64Provider? idStringProvider)
    {
        Debug.Assert(bundleHeader is not null);
        Debug.Assert(resources is not null);

        _assetStream = assetStream;
        _idStringProvider = idStringProvider ?? IDStringRepository.Shared;

        Version = bundleHeader.Version;
        Size = bundleHeader.Size;
        IsCompressed = bundleHeader.IsCompressed;
        _properties = bundleHeader.Properties.ToArray();
        _resourcesMeta = bundleHeader.ResourceMetas.ToArray();
        _resources = resources;

        Resources = _resources.Select(r => new BundleResource(r)).ToArray().AsReadOnlyEx();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _assetStream?.Dispose();
        _disposed = true;
    }

    #region Static Methods

    public static IChunkDecompressionStrategy GetDecompressorForVersion(BundleVersion version, byte[]? decompressionDictionary)
    {
        return version switch
        {
            BundleVersion.VT2XC => GetZstdDecompressionStrategy(),
            >= BundleVersion.VT1 and < BundleVersion.VT2XC => new ZlibChunkDecompressionStrategy(),
            _ => throw new ArgumentOutOfRangeException($"Unsupported bundle version '{(int)version:X8}'")
        };

        IChunkDecompressionStrategy GetZstdDecompressionStrategy()
        {
            if (decompressionDictionary is null)
                throw new ArgumentException($"Must provide a Zstd decompression dictionary for bundle versions '{BundleVersion.VT2XC}' and above.");

            return new ZstdChunkDecompressionStrategy(decompressionDictionary);
        }
    }

    public static Bundle OpenBundle(string bundlePath, IChunkDecompressionStrategy decompressor, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundlePath);
        using var fs = File.OpenRead(bundlePath);
        // TODO: find and open asset stream
        return CoreOpenBundle(fs, decompressor, null, idString64Provider);
    }

    public static Bundle OpenBundle(Stream bundleStream, IChunkDecompressionStrategy decompressor, Stream? bundleAssetStream = null, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        return CoreOpenBundle(bundleStream, decompressor, bundleAssetStream, idString64Provider);
    }

    private static Bundle CoreOpenBundle(Stream bundleStream, IChunkDecompressionStrategy decompressor, Stream? bundleAssetStream, IIDString64Provider? idString64Provider)
    {
        Debug.Assert(bundleStream is not null);
        idString64Provider ??= IDStringRepository.Shared;

        var (version, size, isCompressed) = ReadBundleMeta(bundleStream);
        using Stream wrapperStream = CreateWrapperStream(bundleStream, isCompressed, decompressor);
        using var reader = new PrimitiveReader(wrapperStream);

        idString64Provider ??= IDStringRepository.Shared;
        var (properties, resourceMetas) = ReadBundlePropsAndResourceMetas(reader, idString64Provider);

        var bundleHeader = new BundleHeader
        {
            Version = version,
            Size = size,
            ResourceCount = resourceMetas.Length,
            Properties = properties.AsReadOnlyEx(),
            ResourceMetas = resourceMetas.AsReadOnlyEx(),
            IsCompressed = isCompressed
        };

        BundledResource[] resources = new BundledResource[bundleHeader.ResourceCount];
        for (int i = 0; i < resources.Length; i++)
            resources[i] = reader.ReadBundledResource(idString64Provider);

        return new Bundle(bundleAssetStream, bundleHeader, resources, idString64Provider);
    }

    public static BundleVersion ReadBundleVersion(string bundlePath)
    {
        using var fs = File.OpenRead(bundlePath);
        return CoreReadBundleVersion(fs);
    }

    public static BundleVersion ReadBundleVersion(Stream bundleStream)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        return CoreReadBundleVersion(bundleStream);
    }

    private static BundleVersion CoreReadBundleVersion(Stream bundleStream)
    {
        Debug.Assert(bundleStream is not null);
        Span<byte> buffer = stackalloc byte[4];
        bundleStream.ReadExactly(buffer);
        return (BundleVersion)BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }

    /// <summary>
    /// Opens the specified bundle and reads its header metadata.
    /// </summary>
    /// <param name="bundlePath">The path to the bundle file.</param>
    /// <param name="idString64Provider">The <see cref="IIDString64Provider"/> to use for looking up string hashes within the bundle header.</param>
    /// <returns>The bundle header that was read.</returns>
    public static BundleHeader ReadBundleHeader(string bundlePath, IChunkDecompressionStrategy decompressor, IIDString64Provider? idString64Provider = null)
    {
        using var fs = File.OpenRead(bundlePath);
        return CoreReadBundleHeader(fs, decompressor, idString64Provider);
    }

    /// <summary>
    /// Reads a bundle's header metadata from the given stream.
    /// </summary>
    /// <param name="bundleStream">The stream containing the bundle to read the header of. Must be seekable.</param>
    /// <param name="idString64Provider">The <see cref="IIDString64Provider"/> to use for looking up string hashes within the bundle header.</param>
    /// <returns>The bundle header that was read.</returns>
    public static BundleHeader ReadBundleHeader(Stream bundleStream, IChunkDecompressionStrategy decompressor, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        // We need to be able to seek so we can compare against the length of the stream to determine whether it's compressed.
        if (!bundleStream.CanSeek)
            throw new ArgumentException("The stream is not seekable.", nameof(bundleStream));

        return CoreReadBundleHeader(bundleStream, decompressor, idString64Provider);
    }

    private static BundleHeader CoreReadBundleHeader(Stream bundleStream, IChunkDecompressionStrategy decompressor, IIDString64Provider? idString64Provider)
    {
        var (version, size, isCompressed) = ReadBundleMeta(bundleStream);
        using Stream wrapperStream = CreateWrapperStream(bundleStream, isCompressed, decompressor);
        using var reader = new PrimitiveReader(wrapperStream);

        idString64Provider ??= IDStringRepository.Shared;
        var (properties, resourceMetas) = ReadBundlePropsAndResourceMetas(reader, idString64Provider);

        return new BundleHeader
        {
            Version = version,
            Size = size,
            ResourceCount = resourceMetas.Length,
            Properties = properties.AsReadOnlyEx(),
            ResourceMetas = resourceMetas.AsReadOnlyEx(),
            IsCompressed = isCompressed
        };
    }

    public static bool IsBundleCompressed(string bundlePath)
    {
        ArgumentNullException.ThrowIfNull(bundlePath);
        using var fs = File.OpenRead(bundlePath);
        return CoreIsBundleCompressed(fs);
    }

    public static bool IsBundleCompressed(Stream bundleStream)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        if (!bundleStream.CanSeek)
            throw new ArgumentException("The stream is not seekable.", nameof(bundleStream));

        long origPos = bundleStream.Position;
        bool isCompressed = CoreIsBundleCompressed(bundleStream);
        bundleStream.Position = origPos;
        return isCompressed;
    }

    private static bool CoreIsBundleCompressed(Stream bundleStream)
    {
        Debug.Assert(bundleStream is not null);
        Span<byte> buffer = stackalloc byte[12];
        bundleStream.ReadExactly(buffer);

        uint version = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        if (version < 0xF000_0000) // We can support any valid bundle version, so we only do a sanity check for a possibly-valid version.
            throw new InvalidDataException("The stream is not a valid bundle file.");

        long uncompressedSize = BinaryPrimitives.ReadInt64LittleEndian(buffer[4..]);
        return bundleStream.Length < uncompressedSize; // If uncompressed, the size of the bundle file will be >= the uncompressed size (due to zero-padding at the end).
    }

    private static (BundleVersion Version, long Size, bool IsCompressed) ReadBundleMeta(Stream bundleStream)
    {
        Span<byte> header = stackalloc byte[12];
        int bytesRead = bundleStream.ReadAtLeast(header, header.Length, false);
        if (bytesRead < header.Length)
            throw new InvalidDataException("The stream is too small to be a valid bundle file.");

        BundleVersion version = (BundleVersion)BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (!SupportedVersions.Contains(version))
            throw new InvalidDataException($"The bundle version '{(uint)version:X8}' is not supported.");

        long size = BinaryPrimitives.ReadInt64LittleEndian(header[4..]);
        bool isCompressed = bundleStream.Length < size;
        return (version, size, isCompressed);
    }

    private static (IDString64[] Properties, BundledResourceMeta[] ResourceMetas) ReadBundlePropsAndResourceMetas(PrimitiveReader reader, IIDString64Provider idString64Provider)
    {
        int resourceCount = reader.ReadInt32LE();

        var properties = new IDString64[PropertyCount];
        for (int i = 0; i < properties.Length; i++)
            properties[i] = reader.ReadIDString64(idString64Provider);

        Debug.Assert(properties.Length == PropertyCount);
        var resourceDescriptors = new BundledResourceMeta[resourceCount];
        for (int i = 0; i < resourceCount; i++)
            resourceDescriptors[i] = reader.ReadBundledResourceMeta(idString64Provider);

        Debug.Assert(resourceDescriptors.Length == resourceCount);
        return (properties, resourceDescriptors);
    }

    private static Stream CreateWrapperStream(Stream bundleStream, bool isCompressed, IChunkDecompressionStrategy decompressor)
    {
        ICompressedChunkReader chunkReader = new CompressedChunkReader(bundleStream, true, decompressor);
        Stream maybeCompressedStream = isCompressed ? new CompressedChunkDecompressionStream(chunkReader) : bundleStream;
        return new LeaveOpenStream(maybeCompressedStream);
    }

    // This isn't foolproof; if a bundle starts with an uncompressed chunk this would assume the entire bundle is uncompressed.
    // Not entirely sure of a better way other than letting the caller indicate whether the bundle is compressed.
    //
    // me now to me in the past: uh, hello, we have the uncompressed size and chunks are only compressed when they'd be smaller - just compare the overall file's size
    // to the size in literally the second number in the bundle header and if it's smaller then it's compressed! idiot! :c
    private static bool CheckForCompressedFirstChunk(Stream bundleStream)
    {
        Debug.Assert(bundleStream.CanSeek);
        Debug.Assert(bundleStream.Length >= 18);

        long origPos = bundleStream.Position;

        Span<byte> buffer = stackalloc byte[6];
        bundleStream.ReadExactly(buffer);

        // If compressed, this is the size of the first compressed chunk;
        // otherwise, this is the bundle's resource count.
        int chunkSizeOrResCount = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        short maybeZLibHeader = BinaryPrimitives.ReadInt16LittleEndian(buffer[4..]); // Note: the zlib (rfc 1950) header is big-endian. 0x789C.
        bool isCompressed = maybeZLibHeader == ZlibUtil.ChunkHeader;

        if (isCompressed && (chunkSizeOrResCount <= 0 || chunkSizeOrResCount > ZlibUtil.MaxChunkLength))
            throw new InvalidDataException($"Invalid first Zlib chunksize read for compressed bundle.");
        if (!isCompressed && (chunkSizeOrResCount <= 0))
            throw new InvalidDataException($"Invalid resource count read for uncompressed bundle.");

        bundleStream.Position = origPos;
        return isCompressed;
    }

    #endregion Static Methods
}