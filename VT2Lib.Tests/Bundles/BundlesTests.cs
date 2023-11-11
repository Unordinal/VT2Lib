using System.Text.RegularExpressions;
using VT2Lib.Bundles;
using VT2Lib.Core.Stingray;
using Xunit.Abstractions;

namespace VT2Lib.Tests.Bundles;

public partial class BundlesTests
{
    private readonly ITestOutputHelper _output;

    public BundlesTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(TestUtils.GetMixedBundleNames), MemberType = typeof(TestUtils))]
    public void ReadBundleMeta(string bundleName)
    {
        bundleName = Path.Combine(TestPaths.MixedBundleFilesPath, bundleName);

        var bundleMeta = Bundle.ReadBundleMeta(bundleName);
        _output.WriteLine(bundleMeta.ToString());
    }

    [Theory]
    [MemberData(nameof(TestUtils.GetMixedBundleNames), MemberType = typeof(TestUtils))]
    public void ReadBundleHeader(string bundleName)
    {
        HashDictUtil.PrepareKnownHashes();

        bundleName = Path.Combine(TestPaths.MixedBundleFilesPath, bundleName); // Purely so it shows up clearly in the Test Explorer.
        //var decompressor = Bundle.GetDecompressorForVersion(BundleVersion.VT2X, CompressionDictPath);

        var header = Bundle.ReadBundleHeader(bundleName);
        _output.WriteLine($"[[Bundle: {Path.GetFileName(bundleName)} ({TestUtils.HumanizeBytes(header.Size)}, {TestUtils.HumanizeBytes(new FileInfo(bundleName).Length)} compressed)]]");
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

    [Theory]
    [MemberData(nameof(TestUtils.GetMixedBundleNames), MemberType = typeof(TestUtils))]
    public void OpenBundle(string bundleName)
    {
        bundleName = Path.Combine(TestPaths.MixedBundleFilesPath, bundleName);
        HashDictUtil.PrepareKnownHashes();

        //var chunkDecompressor = Bundle.GetDecompressorForVersion(BundleVersion.VT2X, (string?)null);
        var bundle = Bundle.OpenBundle(bundleName);
        _output.WriteLine($"[[Bundle: {Path.GetFileName(bundleName)} ({bundle.ResourcesMeta.Count} resources, {TestUtils.HumanizeBytes(bundle.Size)} total)]]");
    }

    [Theory]
    [MemberData(nameof(GetMixedBundleNamesLimited), null)]
    public void ExtractResources(string bundleName)
    {
        bundleName = Path.Combine(TestPaths.MixedBundleFilesPath, bundleName);
        HashDictUtil.PrepareKnownHashes();

        var bundle = Bundle.OpenBundle(bundleName);
        var directory = Directory.CreateDirectory(".extracted_resources_test");

        foreach (var resource in bundle.Resources)
        {
            bundle.ExtractResource(resource, directory.Name);
        }
    }

    public static IEnumerable<object[]> GetMixedBundleNamesLimited(int? count = null)
    {
        var bundleNames = TestUtils.GetMixedBundleNames();
        if (count.HasValue)
            bundleNames = bundleNames.Take(count.Value);

        return bundleNames;
    }
}