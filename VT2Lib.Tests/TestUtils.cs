using System.Globalization;
using System.Text.RegularExpressions;
using VT2Lib.Core.Collections;

namespace VT2Lib.Tests;

internal static partial class TestUtils
{
    public static string HumanizeBytes(long value)
    {
        string suffix;
        double readable;
        switch (Math.Abs(value))
        {
            case >= 0x1000000000000000:
                suffix = "EiB";
                readable = value >> 50;
                break;

            case >= 0x4000000000000:
                suffix = "PiB";
                readable = value >> 40;
                break;

            case >= 0x10000000000:
                suffix = "TiB";
                readable = value >> 30;
                break;

            case >= 0x40000000:
                suffix = "GiB";
                readable = value >> 20;
                break;

            case >= 0x100000:
                suffix = "MiB";
                readable = value >> 10;
                break;

            case >= 0x400:
                suffix = "KiB";
                readable = value;
                break;

            default:
                return value.ToString("0 B");
        }

        return (readable / 1024).ToString("0.## ", CultureInfo.InvariantCulture) + suffix;
    }

    public static IEnumerable<object[]> GetTestBundleNames(string bundlesPath)
    {
        Assert.True(Directory.Exists(bundlesPath));
        foreach (var filePath in Directory.EnumerateFiles(bundlesPath, "*", SearchOption.AllDirectories).Where(IsBundleFile))
            yield return new object[] { Path.GetRelativePath(bundlesPath, filePath) };
    }

    public static IEnumerable<object[]> GetMixedBundleNames()
    {
        return GetTestBundleNames(TestPaths.MixedBundleFilesPath);
    }
    
    public static IEnumerable<object[]> GetTestManyBundleNames()
    {
        return GetTestBundleNames(TestPaths.TestManyBundleFilesPath);
    }

    public static bool IsRestOfStreamZeroes(Stream stream)
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

    public static bool IsBundleFile(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        return BundleNameRegex().IsMatch(fileName);
    }

    [GeneratedRegex(@"[a-z0-9]{16}(?>\.patch_\d{3})?$")]
    private static partial Regex BundleNameRegex();
}