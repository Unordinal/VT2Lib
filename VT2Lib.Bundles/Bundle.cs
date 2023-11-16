using System.Buffers.Binary;
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
using VT2Lib.Core.Stingray.IO.Extensions;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Bundles;

public sealed class Bundle : IDisposable
{
    private const int PropertySectionSize = PropertyCount * sizeof(ulong);
    private const int PropertyCount = 32; // TODO: Figure out what the heck bundle properties even are

    /// <summary>
    /// Gets an array of the supported bundle versions. Versions not in this array cannot be read.
    /// </summary>
    public static ImmutableArray<BundleVersion> SupportedVersions { get; } = new BundleVersion[]
    {
        //BundleVersion.VT2,
        BundleVersion.VT2X,
        BundleVersion.VT2XZtd
    }.ToImmutableArray();

    /// <summary>
    /// Gets the absolute path of the bundle's file name, if it was created via path.
    /// Returns <see langword="null"/> if this bundle was created via stream.
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Gets the bundle's file format version.
    /// </summary>
    public BundleVersion Version { get; }

    /// <summary>
    /// Gets the uncompressed size of the bundle, in bytes.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Gets whether the bundle is compressed.
    /// </summary>
    public bool IsCompressed { get; }

    /// <summary>
    /// Gets the bundle properties.
    /// <para/>
    /// See <see href="http://bitsquid.blogspot.com/2016/09/a-new-localization-system-for-stingray.html"/>
    /// </summary>
    public IReadOnlyList<IDString64> Properties => _properties.AsReadOnlyEx();

    /// <summary>
    /// Gets a list containing the metadata of each resource in the bundle.
    /// </summary>
    public IReadOnlyList<BundledResourceMeta> ResourcesMeta => _resourcesMeta.AsReadOnlyEx();

    /// <summary>
    /// Gets a list of the resources in the bundle.
    /// </summary>
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

    private Bundle(string? fileName, Stream? assetStream, BundleHeader bundleHeader, BundledResource[] resources, IIDString64Provider idStringProvider)
    {
        Debug.Assert(bundleHeader is not null);
        Debug.Assert(resources is not null);

        if (fileName is not null)
            FileName = Path.GetFullPath(fileName);

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

    /// <summary>
    /// Extracts the specified resource from the bundle, including every variant.
    /// </summary>
    /// <param name="resource"></param>
    public void ExtractResource(BundleResource resource, string outputDirectory)
    {
        IDString64 typeId = resource.ResourceLocator.Type;
        IDString64 nameId = resource.ResourceLocator.Name;
        string typeStr = typeId.Value ?? typeId.ID.ToString("x16");
        string nameStr = nameId.Value ?? nameId.ID.ToString("x16");

        bool appendVariantType = resource.Variants.Count > 1;
        for (int i = 0; i < resource.Variants.Count; i++)
        {
            var variant = resource.Variants[i];
            string variantFormat = appendVariantType ? "{0}.{2}.{1}" : "{0}.{1}";
            string variantName = string.Format(variantFormat, nameStr, typeStr, variant.Language);

            string variantPath = Path.Combine(outputDirectory, variantName);
            ExtractResource(variant, variantPath);
        }
    }

    /// <summary>
    /// Extracts the specified resource variant from the bundle.
    /// </summary>
    /// <param name="resourceVariant"></param>
    /// <param name="variantIndex"></param>
    public void ExtractResource(BundleResourceVariant resourceVariant, string outputFilePath)
    {
        if (resourceVariant.Size + resourceVariant.StreamSize == 0)
            return;

        BundleResource resource = resourceVariant.ParentResource;

        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
        using var outputStream = File.Create(outputFilePath);
        outputStream.Write(resourceVariant.Data);

        if (_assetStream is not null && resourceVariant.StreamSize > 0)
        {
            _assetStream.Position = resource.StreamOffset;
            // FIXME: Multiple variants might have stream data;
            // add up the previous variant sizes to get the offset of the current variant?
            _assetStream.CopySomeTo(outputStream, resourceVariant.StreamSize);
        }
    }

    /*public IEnumerable<IResource> GetResources(ResourceReaderRepository? binaryReaderFactory = null)
    {
        binaryReaderFactory ??= ResourceReaderRepository.SharedBinaryReaders;
        foreach (var resource in Resources)
        {
            foreach (var variant in resource.Variants)
            {
                var data = variant.Data;
                binaryReaderFactory.TryGet(resource.ResourceLocator.Type, out var reader);
                IResource srResource = reader!.Read(new MemoryStream(data.ToArray()));
                yield return srResource;
            }
        }
    }*/

    public IEnumerable<byte[]> GetResourceData(BundleResourceVariant resourceVariant)
    {
        yield break;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _assetStream?.Dispose();
        _disposed = true;
    }

    #region Static Methods

    #region Open Bundle

    public static Bundle OpenBundle(string bundlePath, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundlePath);
        using var bundleStream = File.OpenRead(bundlePath);
        var bundleAssetStream = FindAssetStreamForBundle(bundlePath);

        IChunkDecompressor GetDecompressorFunc(BundleVersion version)
        {
            string bundleDir = Path.GetDirectoryName(bundlePath)!;
            string compressionDictPath = Path.Combine(bundleDir, "compression.dictionary");

            byte[]? compressionDictionary = null;
            if (ChunkDecompressorFactory.VersionUsesZstd(version))
            {
                if (!File.Exists(compressionDictPath))
                    throw new FileNotFoundException(
                        "Bundle version requires Zstd compression dictionary, " +
                        "but couldn't find the 'compression.dictionary' file in the bundle's path. " +
                        "Provide one manually via an alternative OpenBundle() overload.");

                compressionDictionary = File.ReadAllBytes(compressionDictPath);
            }

            return GetDecompressorForVersion(version, compressionDictionary);
        }

        return CoreOpenBundle(bundleStream, bundlePath, GetDecompressorFunc, bundleAssetStream, idString64Provider);
    }

    public static Bundle OpenBundle(string bundlePath, ReadOnlyMemory<byte> compressionDictionary, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundlePath);
        using var bundleStream = File.OpenRead(bundlePath);
        var bundleAssetStream = FindAssetStreamForBundle(bundlePath);

        return CoreOpenBundle(bundleStream, bundlePath, (version) => GetDecompressorForVersion(version, compressionDictionary.Span), bundleAssetStream, idString64Provider);
    }

    public static Bundle OpenBundle(string bundlePath, IChunkDecompressor decompressor, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundlePath);
        using var bundleStream = File.OpenRead(bundlePath);
        var bundleAssetStream = FindAssetStreamForBundle(bundlePath);

        return CoreOpenBundle(bundleStream, bundlePath, (_) => decompressor, bundleAssetStream, idString64Provider);
    }

    public static Bundle OpenBundle(Stream bundleStream, ReadOnlyMemory<byte> compressionDictionary, Stream? bundleAssetStream = null, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        return CoreOpenBundle(bundleStream, null, (version) => GetDecompressorForVersion(version, compressionDictionary.Span), bundleAssetStream, idString64Provider);
    }

    public static Bundle OpenBundle(Stream bundleStream, IChunkDecompressor decompressor, Stream? bundleAssetStream = null, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        return CoreOpenBundle(bundleStream, null, (_) => decompressor, bundleAssetStream, idString64Provider);
    }

    private static Bundle CoreOpenBundle(
        Stream bundleStream,
        string? bundlePath,
        Func<BundleVersion, IChunkDecompressor> getDecompressorFunc,
        Stream? bundleAssetStream,
        IIDString64Provider? idString64Provider)
    {
        Debug.Assert(bundleStream is not null);

        var (version, size, isCompressed) = CoreReadBundleMeta(bundleStream);
        var decompressor = getDecompressorFunc(version);

        using Stream wrapperStream = CreateWrapperStream(bundleStream, isCompressed, decompressor);
        PrimitiveReader reader = new(wrapperStream);

        idString64Provider ??= IDStringRepository.Shared;
        var (properties, resourceMetas) = ReadBundlePropsAndResourceMetas(ref reader, idString64Provider);

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

        return new Bundle(bundlePath, bundleAssetStream, bundleHeader, resources, idString64Provider);
    }

    #endregion Open Bundle

    #region Read Meta/Header

    public static (BundleVersion Version, long Size, bool IsCompressed) ReadBundleMeta(string bundlePath)
    {
        using var bundleStream = File.OpenRead(bundlePath);
        return CoreReadBundleMeta(bundleStream);
    }

    /*public static (BundleVersion Version, long Size, bool IsCompressed) ReadBundleMeta(Stream bundleStream)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        return CoreReadBundleMeta(bundleStream);
    }*/

    private static (BundleVersion Version, long Size, bool IsCompressed) CoreReadBundleMeta(Stream bundleStream)
    {
        Debug.Assert(bundleStream is not null);

        Span<byte> header = stackalloc byte[12];
        int bytesRead = bundleStream.ReadAtLeast(header, header.Length, false);
        if (bytesRead < header.Length)
            throw new InvalidDataException("The stream is too small to be a valid bundle file.");

        BundleVersion version = (BundleVersion)BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (version < (BundleVersion)0xF000_0000) // We can most likely support any valid future bundle version with this method, so we only do a sanity check for a possibly-valid version.
            throw new InvalidDataException("The file is not a valid bundle file.");

        long uncompressedSize = BinaryPrimitives.ReadInt64LittleEndian(header[4..]);
        if (uncompressedSize <= 0)
            throw new InvalidDataException("The bundle file has an invalid uncompressed size.");

        bool isCompressed = bundleStream.Length < uncompressedSize;
        return (version, uncompressedSize, isCompressed);
    }

    /// <summary>
    /// Opens the specified bundle and reads its header metadata.
    /// </summary>
    /// <param name="bundlePath">The path to the bundle file.</param>
    /// <param name="idString64Provider">The <see cref="IIDString64Provider"/> to use for looking up string hashes within the bundle header.</param>
    /// <returns>The bundle header that was read.</returns>
    public static BundleHeader ReadBundleHeader(string bundlePath, IIDString64Provider? idString64Provider = null)
    {
        using var bundleStream = File.OpenRead(bundlePath);

        IChunkDecompressor GetDecompressorFunc(BundleVersion version)
        {
            string bundleDir = Path.GetDirectoryName(bundlePath)!;
            string compressionDictPath = Path.Combine(bundleDir, "compression.dictionary");

            byte[]? compressionDictionary = null;
            if (ChunkDecompressorFactory.VersionUsesZstd(version))
            {
                if (!File.Exists(compressionDictPath))
                    throw new FileNotFoundException("Bundle version requires Zstd compression dictionary, but couldn't find the 'compression.dictionary' file in the bundle's path.");

                compressionDictionary = File.ReadAllBytes(compressionDictPath);
            }

            return GetDecompressorForVersion(version, compressionDictionary);
        }

        return CoreReadBundleHeader(bundleStream, GetDecompressorFunc, idString64Provider);
    }

    /// <summary>
    /// Opens the specified bundle and reads its header metadata.
    /// </summary>
    /// <param name="bundlePath">The path to the bundle file.</param>
    /// <param name="idString64Provider">The <see cref="IIDString64Provider"/> to use for looking up string hashes within the bundle header.</param>
    /// <returns>The bundle header that was read.</returns>
    public static BundleHeader ReadBundleHeader(string bundlePath, ReadOnlyMemory<byte> compressionDictionary, IIDString64Provider? idString64Provider = null)
    {
        using var bundleStream = File.OpenRead(bundlePath);
        return CoreReadBundleHeader(bundleStream, (version) => GetDecompressorForVersion(version, compressionDictionary.Span), idString64Provider);
    }

    /// <summary>
    /// Opens the specified bundle and reads its header metadata.
    /// </summary>
    /// <param name="bundlePath">The path to the bundle file.</param>
    /// <param name="idString64Provider">The <see cref="IIDString64Provider"/> to use for looking up string hashes within the bundle header.</param>
    /// <returns>The bundle header that was read.</returns>
    public static BundleHeader ReadBundleHeader(string bundlePath, IChunkDecompressor decompressor, IIDString64Provider? idString64Provider = null)
    {
        using var bundleStream = File.OpenRead(bundlePath);
        return CoreReadBundleHeader(bundleStream, (_) => decompressor, idString64Provider);
    }

    /// <summary>
    /// Reads a bundle's header metadata from the given stream.
    /// </summary>
    /// <param name="bundleStream">The stream containing the bundle to read the header of. Must be seekable.</param>
    /// <param name="idString64Provider">The <see cref="IIDString64Provider"/> to use for looking up string hashes within the bundle header.</param>
    /// <returns>The bundle header that was read.</returns>
    public static BundleHeader ReadBundleHeader(Stream bundleStream, ReadOnlyMemory<byte> compressionDictionary, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        // We need to be able to seek so we can compare against the length of the stream to determine whether it's compressed.
        if (!bundleStream.CanSeek)
            throw new ArgumentException("The stream is not seekable.", nameof(bundleStream));

        return CoreReadBundleHeader(bundleStream, (version) => GetDecompressorForVersion(version, compressionDictionary.Span), idString64Provider);
    }

    /// <summary>
    /// Reads a bundle's header metadata from the given stream.
    /// </summary>
    /// <param name="bundleStream">The stream containing the bundle to read the header of. Must be seekable.</param>
    /// <param name="idString64Provider">The <see cref="IIDString64Provider"/> to use for looking up string hashes within the bundle header.</param>
    /// <returns>The bundle header that was read.</returns>
    public static BundleHeader ReadBundleHeader(Stream bundleStream, IChunkDecompressor decompressor, IIDString64Provider? idString64Provider = null)
    {
        ArgumentNullException.ThrowIfNull(bundleStream);
        // We need to be able to seek so we can compare against the length of the stream to determine whether it's compressed.
        if (!bundleStream.CanSeek)
            throw new ArgumentException("The stream is not seekable.", nameof(bundleStream));

        return CoreReadBundleHeader(bundleStream, (_) => decompressor, idString64Provider);
    }

    private static BundleHeader CoreReadBundleHeader(Stream bundleStream, Func<BundleVersion, IChunkDecompressor> getDecompressorFunc, IIDString64Provider? idString64Provider)
    {
        var (version, size, isCompressed) = CoreReadBundleMeta(bundleStream);
        var decompressor = getDecompressorFunc(version);

        using Stream wrapperStream = CreateWrapperStream(bundleStream, isCompressed, decompressor);
        PrimitiveReader reader = new(wrapperStream);

        idString64Provider ??= IDStringRepository.Shared;
        var (properties, resourceMetas) = ReadBundlePropsAndResourceMetas(ref reader, idString64Provider);

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

    private static (IDString64[] Properties, BundledResourceMeta[] ResourceMetas) ReadBundlePropsAndResourceMetas(ref PrimitiveReader reader, IIDString64Provider idString64Provider)
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

    #endregion Read Meta/Header

    #region Private Helpers

    private static Stream CreateWrapperStream(Stream bundleStream, bool isCompressed, IChunkDecompressor? decompressor)
    {
        Stream resultStream = bundleStream;
        if (isCompressed)
        {
            if (decompressor is null)
                throw new ArgumentNullException(nameof(decompressor), $"Bundle is compressed but no chunk decompressor was passed.");

            ICompressedChunkReader chunkReader = new CompressedChunkReader(bundleStream, true, decompressor);
            resultStream = new CompressedChunkDecompressionStream(chunkReader);
        }
        return new LeaveOpenStream(resultStream);
    }

    private static Stream? FindAssetStreamForBundle(string bundlePath)
    {
        Debug.Assert(bundlePath is not null);
        string assetStreamPath = bundlePath + ".stream";

        return File.Exists(assetStreamPath) ? File.OpenRead(assetStreamPath) : null;
    }

    internal static IChunkDecompressor GetDecompressorForVersion(BundleVersion version, string? compressionDictionaryPath = null)
    {
        Span<byte> buffer = null;
        if (File.Exists(compressionDictionaryPath))
            buffer = File.ReadAllBytes(compressionDictionaryPath);

        return GetDecompressorForVersion(version, buffer);
    }

    internal static IChunkDecompressor GetDecompressorForVersion(BundleVersion version, ReadOnlySpan<byte> compressionDictionary)
    {
        return ChunkDecompressorFactory.Create(version, compressionDictionary);
    }

    #endregion Private Helpers

    #endregion Static Methods
}