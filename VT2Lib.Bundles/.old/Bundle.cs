using System.Collections.Immutable;
using VT2Lib.Bundles.Resources;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;

namespace VT2Lib.Bundles;

public sealed class Bundle : IDisposable
{
    public const int MaxPropertyCount = 32; // TODO: Figure out what the heck bundle properties even are
    internal const int PropertySectionSize = MaxPropertyCount * sizeof(ulong);
    internal const ushort ZlibHeader = 0x789C;
    private const int FirstZlibHeaderPos = 4 + 8 + 4; // sizeof version + sizeof bundleSize + sizeof chunkSize;

    public static ImmutableArray<BundleVersion> SupportedVersions { get; } = new()
    {
        //BundleVersion.VT2,
        BundleVersion.VT2X
    };

    public BundleVersion Version { get; }

    public long Size { get; }

    public bool IsCompressed { get; }

    public ImmutableArray<IDString64> Properties { get; private set; }

    public ImmutableArray<IDString64> ResourceList { get; private set; }

    /// <summary>
    /// Gets this bundle's associated .stream file, if it has one.
    /// </summary>
    public Stream? StreamFile { get; private set; }

    private readonly Stream _stream;
    private readonly Stream? _bundleStreamFile;
    private readonly IIDString64Provider _idStringProvider;
    private readonly IDString64[] _properties;
    private BundledResourceMeta[] _resourceMetas;
    private bool _disposed;

    private Bundle(
        Stream stream, 
        Stream? bundleStreamFile, 
        BundleVersion version,
        long size,
        bool isCompressed,
        IIDString64Provider? idStringProvider = null)
    {
        _stream = stream;
        _bundleStreamFile = bundleStreamFile;
        Version = version;
        Size = size;
        IsCompressed = isCompressed;
        _idStringProvider = idStringProvider ?? IDStringRepository.Shared;
    }

    public void Dispose()
    {
    }

    private void ReadProperties
}