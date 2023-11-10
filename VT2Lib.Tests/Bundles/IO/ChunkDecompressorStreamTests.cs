using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;
using VT2Lib.Bundles;
using VT2Lib.Bundles.IO;
using VT2Lib.Bundles.IO.Compression;
using VT2Lib.Core.Collections;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray;
using VT2Lib.Tests.Attributes;
using Xunit.Abstractions;

namespace VT2Lib.Tests.Bundles.IO;

public partial class ChunkDecompressionStreamTests
{
    private static readonly string TestBundleFilesPath = Path.Combine(ProjectSource.ProjectDirectory, @"TestFiles\Bundles\");

    private readonly ITestOutputHelper _output;

    public ChunkDecompressionStreamTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(ReadTestBundleFiles))]
    public void CDS_ReadBundlesHeader(string bundleName)
    {
        HashDictUtil.PrepareKnownHashes();

        bundleName = GetPathForBundle(bundleName); // Purely so it shows up clearly in the Test Explorer.
        var header = Bundle.ReadBundleHeader(bundleName);
        _output.WriteLine($"[[Bundle: {Path.GetFileName(bundleName)} ({MiscUtil.HumanizeBytes(header.Size)})]]");
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
            _output.WriteLine($"\t{resLoc.Name} ({MiscUtil.HumanizeBytes(resource.TotalSize)}; Flag: {resource.Flag})");
        }
        _output.WriteLine("");
    }

    [Fact]
    public void CDS_ReadNewBundleHeaderTest()
    {
        HashDictUtil.PrepareKnownHashes();

        string bundleName = @"G:\Games\Steam\steamapps\common\Warhammer Vermintide 2\bundle\00a353ad557df55f";

        var header = Bundle.ReadBundleHeader(bundleName);
        _output.WriteLine($"[[Bundle: {Path.GetFileName(bundleName)} ({MiscUtil.HumanizeBytes(header.Size)})]]");
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
            _output.WriteLine($"\t{resLoc.Name} ({MiscUtil.HumanizeBytes(resource.TotalSize)}; Flag: {resource.Flag})");
        }
        _output.WriteLine("");
    }

    [Theory]
    [MemberDataWithInline(nameof(ReadTestBundleFiles), 1)]
    [MemberDataWithInline(nameof(ReadTestBundleFiles), 50)]
    public void CDS_ReadBundlesChunks(string bundleName, int numChunksToBuffer)
    {
        //_output.WriteLine("Reading bundle " + bundleName);
        bundleName = GetPathForBundle(bundleName); // Purely so it shows up clearly in the Test Explorer.

        using var fs = File.OpenRead(bundleName);
        using var reader = new PrimitiveReader(fs);
        using var decompStream = new CompressedChunkDecompressionStream(fs, numChunksToBuffer);

        uint bundleVersion = reader.ReadUInt32LE();
        ulong bundleUncompressedSize = reader.ReadUInt64LE();

        //_output.WriteLine("Uncompressed bundle size: " + bundleUncompressedSize);

        byte[] bundleData = new byte[bundleUncompressedSize];
        // This cast probably won't be a problem? I don't think any current bundles are >2GB.
        int bytesRead = decompStream.ReadAtLeast(bundleData, (int)bundleUncompressedSize, false);

        Assert.Equal(bundleUncompressedSize, (ulong)bytesRead);
        Assert.True(IsRestOfStreamZeroes(decompStream)); // Last bundle chunk is zero-padded.
        Assert.Throws<EndOfStreamException>(() =>
        {
            decompStream.ReadExactly(stackalloc byte[1]);
        });
    }

    public static IEnumerable<object[]> ReadTestBundleFiles()
    {
        Assert.True(Directory.Exists(TestBundleFilesPath));
        foreach (var filePath in Directory.EnumerateFiles(TestBundleFilesPath).Where(IsBundleFile))
            yield return new object[] { Path.GetFileName(filePath) }; // Purely so it shows up clearly in the Test Explorer.
    }

    private static bool IsRestOfStreamZeroes(Stream stream)
    {
        using RentedArray<byte> buffer = new(4096);
        var bufSpan = buffer.Span;

        int bytesRead;
        while ((bytesRead = stream.Read(bufSpan)) != 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                if (bufSpan[i] != 0)
                    return false;
            }
        }

        return true;
    }

    private static string GetPathForBundle(string bundleName)
    {
        return Path.Combine(TestBundleFilesPath, bundleName);
    }

    private static bool IsBundleFile(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        return BundleNameRegex().IsMatch(fileName);
    }

    [GeneratedRegex(@"[a-z0-9]{16}(?>\.patch_\d{3})?$")]
    private static partial Regex BundleNameRegex();
}