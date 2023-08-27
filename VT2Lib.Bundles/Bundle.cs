using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics;
using VT2Lib.Bundles.Extensions;
using VT2Lib.Bundles.Resources;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Extensions;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Bundles;

// Currently, we don't load the entire bundle into memory and instead gradually decompress from a stream when needed.
// We could instead load the entire thing into memory. This would make a few things a lot easier and mean this wouldn't
// need to implement IDisposable.
public sealed class Bundle : IDisposable
{
    public const int MaxPropertyCount = 32; // TODO: Figure out what the heck bundle properties even are
    internal const int PropertySectionSize = MaxPropertyCount * sizeof(ulong);
    internal const int MaxChunkLength = 0x10000;
    internal const ushort ZlibHeader = 0x789C;
    private const int FirstZlibHeaderPos = 4 + 8 + 4; // sizeof version + sizeof bundleSize + sizeof chunkSize;

    public static readonly IReadOnlyList<BundleVersion> SupportedVersions = ImmutableArray.Create
    (
        BundleVersion.VT2,
        BundleVersion.VT2X
    );

    /// <summary>
    /// Gets the version of the bundle.
    /// </summary>
    public BundleVersion Version { get; }

    /// <summary>
    /// Gets the uncompressed size of the bundle.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Gets whether the bundle's data is compressed.
    /// </summary>
    public bool IsCompressed { get; }

    /// <summary>
    /// Gets the number of resources in the bundle.
    /// Note that a single resource can contain multiple files, such as in the case of localized files.
    /// </summary>
    public int ResourceCount { get; private set; }

    /// <summary>
    /// Gets the bundle's property list.
    /// </summary>
    public IReadOnlyList<IDString64> Properties { get; private set; }

    /// <summary>
    /// Gets the names and types of each resource in the bundle.
    /// </summary>
    public IReadOnlyList<ResourceLocator> ResourceList { get; private set; }

    // We should not seek this stream as we cannot assume it is seekable since it may be a decompressor stream.
    private readonly Stream _stream;

    private readonly IIDString64Provider _idStringProvider;
    private readonly IDString64[] _properties;
    private BundledResourceMeta[] _resourceMetas;
    private int _indexOfLastReadResource;

    // Does no validation. At this point, the Stream should be positioned over the bundle's 'entryCount' in the data.
    private Bundle(BundleVersion version, long size, bool isCompressed, Stream bundleStream, IIDString64Provider? idStringProvider = null)
    {
        Debug.Assert(SupportedVersions.Contains(version), $"Expected supported version, {version} not supported");
        Debug.Assert(size != 0);
        Debug.Assert(bundleStream is not null);

        Version = version;
        Size = size;
        IsCompressed = isCompressed;
        _stream = bundleStream;
        _idStringProvider = idStringProvider ?? IDStringRepository.Shared;

        using var reader = new PrimitiveReader(_stream, true);
        ResourceCount = reader.ReadInt32LE();
        if (ResourceCount < 0)
            throw new InvalidDataException($"Read invalid bundle resource count, {ResourceCount} < 0");

        _properties = new IDString64[MaxPropertyCount];
        for (int i = 0; i < _properties.Length; i++)
            _properties[i] = reader.ReadIDString64(_idStringProvider);

        Properties = _properties.AsReadOnly();
        _resourceMetas = new BundledResourceMeta[ResourceCount];
        for (int i = 0; i < _resourceMetas.Length; i++)
            _resourceMetas[i] = reader.ReadBundledResourceMeta(_idStringProvider);

        ResourceList = _resourceMetas.Select(meta => meta.ResourceLocator).ToImmutableArray();
    }

    public IResource ReadResource(ResourceLocator resourceLocator)
    {
        return ReadResource(resourceLocator, 0);
    }

    public IResource ReadResource(ResourceLocator resourceLocator, int variantIndex)
    {
        if (variantIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(variantIndex));


    }

    public IEnumerable<IResource> ReadResourceVariants(ResourceLocator resourceLocator)
    {

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private void UpdateResourceList()
    {
        ResourceCount = _resourceMetas.Length;
        ResourceList = _resourceMetas.Select(meta => meta.ResourceLocator).ToArray().AsReadOnly();
    }

    public static Bundle Open(string bundlePath, IIDString64Provider? idStringProvider = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(bundlePath);
        var bundleStream = File.OpenRead(bundlePath);

        return Open(bundleStream, idStringProvider);
    }

    public static Bundle Open(Stream stream, IIDString64Provider? idStringProvider = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
            throw new NotSupportedException(
                "Impossible to determine if the bundle is compressed when using a non-seekable stream. " +
                "Use 'Bundle.Open(Stream, bool)' instead.");

        stream.Position = FirstZlibHeaderPos;
        Span<byte> zlibHeaderBytes = stackalloc byte[4];
        stream.ReadExactly(zlibHeaderBytes);

        int readZlibHeader = BinaryPrimitives.ReadInt32LittleEndian(zlibHeaderBytes);
        bool isCompressed = (readZlibHeader == ZlibHeader);

        stream.Position = 0;
        return Open(stream, isCompressed, idStringProvider);
    }

    public static Bundle Open(Stream stream, bool isCompressed, IIDString64Provider? idStringProvider = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        Span<byte> headerBytes = stackalloc byte[12];

        BundleVersion version = (BundleVersion)BinaryPrimitives.ReadUInt32LittleEndian(headerBytes);
        if (!SupportedVersions.Contains(version))
            throw new InvalidDataException($"The bundle version '{version:x8}' is not supported.");

        long size = BinaryPrimitives.ReadInt64LittleEndian(headerBytes[4..]);
        return new Bundle(version, size, isCompressed, stream, idStringProvider);
    }
}