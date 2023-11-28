using System.Text.RegularExpressions;
using VT2Lib.Bundles;
using VT2Lib.Bundles.IO.Compression;
using VT2Lib.Core.Collections;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray;
using VT2Lib.Tests.Attributes;
using Xunit.Abstractions;

namespace VT2Lib.Tests.Bundles.IO;

public partial class ChunkDecompressionStreamTests
{
    private readonly ITestOutputHelper _output;

    public ChunkDecompressionStreamTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(GetMixedBundleNamesLimited), 10)]
    public void CDS_ReadBundlesHeader(string bundleName)
    {
        HashDictUtil.PrepareKnownHashes();

        bundleName = Path.Combine(TestPaths.MixedBundleFilesPath, bundleName); // Purely so it shows up clearly in the Test Explorer.
        //var decompressor = Bundle.GetDecompressorForVersion(BundleVersion.VT2X, (string?)null);

        var header = Bundle.ReadBundleHeader(bundleName);
        _output.WriteLine($"[[Bundle: {Path.GetFileName(bundleName)} ({TestUtils.HumanizeBytes(header.Size)})]]");
        _output.WriteLine($"[Version {header.Version}]");

        string propsList = string.Join(", ", header.Properties.Where(p => p != IDString64.Empty));
        _output.WriteLine($"[{header.Properties.Count} properties: [{propsList}]]");

        _output.WriteLine($"[{header.ResourceCount} resources]");

        IDString64 lastType = default;
        foreach (var resource in header.ResourceMetas.OrderBy(r => r.ResourceLocator.Type))
        {
            if (lastType != resource.ResourceLocator.Type)
            {
                lastType = resource.ResourceLocator.Type;
                _output.WriteLine($"[Resources of type '{lastType}']");
            }
            var resLoc = resource.ResourceLocator;
            _output.WriteLine($"\t{resLoc.Name} ({TestUtils.HumanizeBytes(resource.TotalSize)}; Flag: {resource.Flag})");
        }
        _output.WriteLine("");
    }

    /*[Fact]
    public void CDS_ReadNewBundleHeaderTest()
    {
        HashDictUtil.PrepareKnownHashes();

        string bundleName = @"G:\Games\Steam\steamapps\common\Warhammer Vermintide 2\bundle\00a353ad557df55f";

        var decompressor = Bundle.GetDecompressorForVersion(BundleVersion.VT2XZtd, TestPaths.CompressionDictPath);

        var header = Bundle.ReadBundleHeader(bundleName, decompressor);
        _output.WriteLine($"[[Bundle: {Path.GetFileName(bundleName)} ({TestUtils.HumanizeBytes(header.Size)})]]");
        _output.WriteLine($"[Version {header.Version}]");
        string propsList = string.Join(", ", header.Properties.Where(p => p != IDString64.Empty));
        _output.WriteLine($"[{header.Properties.Count} properties: [{propsList}]]");

        _output.WriteLine($"[{header.ResourceCount} resources]");

        IDString64 lastType = default;
        foreach (var resource in header.ResourceMetas.OrderBy(r => r.ResourceLocator.Type))
        {
            if (lastType != resource.ResourceLocator.Type)
            {
                lastType = resource.ResourceLocator.Type;
                _output.WriteLine($"[Resources of type '{lastType}']");
            }
            var resLoc = resource.ResourceLocator;
            _output.WriteLine($"\t{resLoc.Name} ({TestUtils.HumanizeBytes(resource.TotalSize)}; Flag: {resource.Flag})");
        }
        _output.WriteLine("");
    }*/

    [Theory]
    [MemberData(nameof(GetMixedBundleNamesLimitedBuffer), 10)]
    public void CDS_ReadBundlesChunks(string bundleName, int numChunksToBuffer)
    {
        //_output.WriteLine("Reading bundle " + bundleName);
        bundleName = Path.Combine(TestPaths.MixedBundleFilesPath, bundleName); // Purely so it shows up clearly in the Test Explorer.
        var (version, _, _) = Bundle.ReadBundleMeta(bundleName);
        var decompressor = Bundle.GetDecompressorForVersion(version, TestPaths.GetCompressionDict(bundleName));

        using var fs = File.OpenRead(bundleName);
        var reader = new PrimitiveReader(fs);

        using var compChunkReader = new CompressedChunkReader(fs, true, decompressor);
        using var decompStream = new CompressedChunkDecompressionStream(compChunkReader, numChunksToBuffer);

        uint bundleVersion = reader.ReadUInt32LE();
        ulong bundleUncompressedSize = reader.ReadUInt64LE();

        //_output.WriteLine("Uncompressed bundle size: " + bundleUncompressedSize);

        byte[] bundleData = new byte[bundleUncompressedSize];
        // This cast probably won't be a problem? I don't think any current bundles are >2GB.
        int bytesRead = decompStream.ReadAtLeast(bundleData, (int)bundleUncompressedSize, false);

        Assert.Equal(bundleUncompressedSize, (ulong)bytesRead);
        Assert.True(TestUtils.IsRestOfStreamZeroes(decompStream), "Rest of stream is not zeroes."); // Last bundle chunk is zero-padded.
        Assert.Throws<EndOfStreamException>(() =>
        {
            decompStream.ReadExactly(stackalloc byte[1]);
        });
    }

    public static IEnumerable<object[]> GetMixedBundleNamesLimited(int? count = null)
    {
        var bundleNames = TestUtils.GetMixedBundleNames();
        if (count.HasValue)
            bundleNames = bundleNames.Take(count.Value);

        return bundleNames;
    }
    
    public static IEnumerable<object[]> GetMixedBundleNamesLimitedBuffer(int? count = null)
    {
        var bundleNames = TestUtils.GetMixedBundleNames();
        if (count.HasValue)
            bundleNames = bundleNames.Take(count.Value);

        return (IEnumerable<object[]>)bundleNames.Select(o => o.Zip(o.Select(_ => 2)).ToArray());
    }
}