using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Bundles.Extensions;
using VT2Lib.Bundles.Resources;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Extensions;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Bundles.IO;
internal sealed class BundleStream : IDisposable
{
    public const int MaxPropertyCount = 32; // TODO: Figure out what the heck bundle properties even are
    internal const int PropertySectionSize = MaxPropertyCount * sizeof(ulong);

    /// <summary>
    /// Gets the absolute path to the bundle if this instance was created via file path.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Gets the bundle's version.
    /// </summary>
    public BundleVersion Version { get; }

    /// <summary>
    /// Gets the uncompressed size of the bundle.
    /// </summary>
    public long Size { get; }

    public bool IsCompressed { get; }

    public ImmutableArray<IDString64> Properties { get; }

    public ImmutableArray<ResourceLocator> Resources { get; }

    private readonly Stream _stream;
    private readonly Stream _dataStream;
    private readonly IIDString64Provider _idStringProvider;
    private readonly bool _leaveOpen;
    private bool _disposed;

    private readonly IDString64[] _properties;
    private readonly BundledResourceMeta[] _resourceMetas;

    private readonly Dictionary<ResourceLocator, (int ChunkIndex, long OffsetInChunk)> _resourceOffsetMap = new();

    public BundleStream(string bundlePath)
        : this(File.OpenRead(bundlePath))
    {
        FilePath = Path.GetFullPath(bundlePath);
    }

    public BundleStream(Stream stream, IIDString64Provider? idStringProvider = null, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
            throw new ArgumentException("The given stream does not support seeking.", nameof(stream));

        // Init from arguments
        _stream = stream;
        _idStringProvider = idStringProvider ?? IDStringRepository.Shared;
        _leaveOpen = leaveOpen;

        // Start reading
        Span<byte> headerBytes = stackalloc byte[12];
        _stream.ReadExactly(headerBytes);

        Version = (BundleVersion)BinaryPrimitives.ReadInt32LittleEndian(headerBytes);
        if (!Bundle.SupportedVersions.Contains(Version))
            throw new InvalidDataException($"Unsupported bundle version '{Version}'");

        Size = BinaryPrimitives.ReadInt64LittleEndian(headerBytes[4..]);
        IsCompressed = _stream.Length != Size;

        _dataStream = IsCompressed
            ? new ChunkDecompressorStreamOld(_stream, 2, leaveOpen)
            : _stream;

        using var reader = new PrimitiveReader(_dataStream, true);

        int resourceCount = reader.ReadInt32LE();
        if (resourceCount < 0)
            throw new InvalidDataException(
                $"Read invalid bundle resource count; {resourceCount} < 0"
            );

        _properties = new IDString64[MaxPropertyCount];
        for (int i = 0; i < _properties.Length; i++)
            _properties[i] = reader.ReadIDString64(_idStringProvider);
        Properties = _properties.ToImmutableArray();

        _resourceMetas = new BundledResourceMeta[resourceCount];
        for (int i = 0; i < _resourceMetas.Length; i++)
            _resourceMetas[i] = reader.ReadBundledResourceMeta(_idStringProvider);

        Resources = _resourceMetas.Select(meta => meta.ResourceLocator).ToImmutableArray();
    }

    public IResource ReadResource(ResourceLocator resourceLocator)
    {
        // TODO: check if resource is past current chunk and don't reset/flush if not
        _stream.Position = _resourcesStartPos; // this won't work atm
        _dataStream.Flush();

        // read each resource until we find the one we're looking for
        // ideally create a system that creates and uses an index structure
        // so we can easily look up and seek to nearest chunk and offset to read directly
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (!_leaveOpen)
        {
            _dataStream.Dispose();
            _stream.Dispose();
        }
    }
}
