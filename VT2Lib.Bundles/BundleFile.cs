using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Bundles.Extensions;
using VT2Lib.Bundles.IO;
using VT2Lib.Bundles.Resources;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Extensions;

namespace VT2Lib.Bundles;

// Not entirely sold on this being a separate class from 'Bundle'.
public static class BundleFile
{
    public static Bundle Open(string bundlePath, IIDString64Provider? idStringProvider = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(bundlePath);
        idStringProvider ??= IDStringRepository.Shared;
        var header = ReadBundleHeader()

        var bundleStream = File.OpenRead(bundlePath);
        return new Bundle(bundleStream, idStringProvider);
    }

    /// <summary>
    /// Convenience function to read the bundle header and get <see cref="BundleHeader.ResourceList"/>.
    /// </summary>
    /// <param name="bundlePath"></param>
    /// <param name="idStringProvider"></param>
    /// <returns></returns>
    public static IReadOnlyList<BundledResourceMeta> ReadBundleResourceList(string bundlePath, IIDString64Provider? idStringProvider = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(bundlePath);
        idStringProvider ??= IDStringRepository.Shared;

        var header = ReadBundleHeader(bundlePath, idStringProvider);
        return header.ResourceList;
    }

    public static BundleHeader ReadBundleHeader(string bundlePath, IIDString64Provider? idStringProvider = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(bundlePath);
        using var stream = File.OpenRead(bundlePath);
        return ReadBundleHeader(stream, idStringProvider);
    }

    private static BundleHeader ReadBundleHeader(Stream stream, IIDString64Provider? idStringProvider = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
            throw new NotSupportedException($"The stream is not seekable.");

        stream.Position = 0;
        idStringProvider ??= IDStringRepository.Shared;

        Span<byte> header = stackalloc byte[20];
        int bytesRead = stream.ReadAtLeast(header, 20, false);
        if (bytesRead < 20)
            throw new InvalidDataException("The stream does not contain enough data to be a valid bundle file.");

        BundleVersion version = (BundleVersion)BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (!Bundle.SupportedVersions.Contains(version))
            throw new InvalidDataException($"The bundle version '{version:x8}' is not supported.");

        long size = BinaryPrimitives.ReadInt64LittleEndian(header[4..]);
        // If compressed, this is the size of the first compressed chunk;
        // otherwise, this is the bundle's resource count.
        int chunkSizeOrResCount = BinaryPrimitives.ReadInt32LittleEndian(header[12..]);
        ushort possibleZlibHeader = BinaryPrimitives.ReadUInt16BigEndian(header[16..]); // Note: the zlib (rfc 1950) header is big-endian. 0x789C.
        bool isCompressed = possibleZlibHeader == Bundle.ZlibHeader;

        if (isCompressed && (chunkSizeOrResCount <= 0 || chunkSizeOrResCount > 0x10000))
            throw new InvalidDataException($"Invalid first Zlib chunksize read for compressed bundle.");
        if (!isCompressed && (chunkSizeOrResCount <= 0))
            throw new InvalidDataException($"Invalid resource count read for uncompressed bundle.");

        stream.Position = 12;

        using Stream wrapperStream = isCompressed
            ? new ChunkDecompressorStream(stream, Bundle.MaxChunkLength, 1, true)
            : new LeaveOpenStream(stream); // We want to autodispose the chunk decompressor stream but not the base stream, so we do this.
        using var reader = new PrimitiveReader(wrapperStream);

        int resourceCount = reader.ReadInt32LE();
        // PERF: We could instead use something like the below to read the entire block into a buffer and then manually read the objects from it.
        // Since the stream we're reading from likely buffers data, this probably isn't necessary.
        // It may give improvements to a possible async version, however.
        // int propsAndResDirSize = Bundle.PropertySectionSize + (BundledResourceMeta.GetSizeForBundleVersion(version) * resourceCount);

        IDString64[] properties = new IDString64[Bundle.MaxPropertyCount];
        for (int i = 0; i < properties.Length; i++)
            properties[i] = reader.ReadIDString64(idStringProvider);

        BundledResourceMeta[] resourcesMeta = new BundledResourceMeta[resourceCount];
        for (int i = 0; i < resourcesMeta.Length; i++)
            resourcesMeta[i] = reader.ReadBundledResourceMeta(idStringProvider);

        return new BundleHeader
        {
            Version = version,
            Size = size,
            IsCompressed = isCompressed,
            ResourceCount = resourceCount,
            Properties = properties,
            ResourceList = resourcesMeta.AsReadOnly()
        };
    }
}