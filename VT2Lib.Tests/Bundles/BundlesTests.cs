using System.Text.RegularExpressions;
using VT2Lib.Bundles;
using VT2Lib.Core.Stingray;
using Xunit.Abstractions;

namespace VT2Lib.Tests.Bundles;

public partial class BundlesTests
{
    private static readonly string TestBundleFilesPath = Path.Combine(ProjectSource.ProjectDirectory, @"TestFiles\Bundles\");
    private static readonly string CompressionDictPath = @"G:\Games\Steam\steamapps\common\Warhammer Vermintide 2\bundle\compression.dictionary";

    private readonly ITestOutputHelper _output;

    public BundlesTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(ReadTestBundleFiles))]
    public void ReadBundleVersion(string bundleName)
    {
        bundleName = GetPathForBundle(bundleName);

        var bundleVersion = Bundle.ReadBundleVersion(bundleName);
        Assert.Equal(BundleVersion.VT2X, bundleVersion);
    }

    [Theory]
    [MemberData(nameof(ReadTestBundleFiles))]
    public void ReadBundleHeader(string bundleName)
    {
        HashDictUtil.PrepareKnownHashes();

        bundleName = GetPathForBundle(bundleName); // Purely so it shows up clearly in the Test Explorer.
        var decompressor = Bundle.GetDecompressorForVersion(BundleVersion.VT2X, null);

        var header = Bundle.ReadBundleHeader(bundleName, decompressor);
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
    [MemberData(nameof(ReadTestBundleFiles))]
    public void OpenBundle(string bundleName)
    {
        bundleName = GetPathForBundle(bundleName);

        HashDictUtil.PrepareKnownHashes();

        var chunkDecompressor = Bundle.GetDecompressorForVersion(BundleVersion.VT2X, null);
        var bundle = Bundle.OpenBundle(bundleName, chunkDecompressor);
        _output.WriteLine($"[[Bundle: {Path.GetFileName(bundleName)} ({bundle.ResourcesMeta.Count} resources, {MiscUtil.HumanizeBytes(bundle.Size)} total)]]");
    }

    public static IEnumerable<object[]> ReadTestBundleFiles()
    {
        Assert.True(Directory.Exists(TestBundleFilesPath));
        foreach (var filePath in Directory.EnumerateFiles(TestBundleFilesPath).Where(IsBundleFile))
            yield return new object[] { Path.GetFileName(filePath) }; // Purely so it shows up clearly in the Test Explorer.
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