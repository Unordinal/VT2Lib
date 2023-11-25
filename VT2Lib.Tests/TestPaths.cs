using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VT2Lib.Tests;
internal static class TestPaths
{
    public static readonly string VT2XBundleFilesPath = Path.Combine(ProjectSource.ProjectDirectory, @"TestFiles\Bundles VT2X\");
    public static readonly string MixedBundleFilesPath = Path.Combine(ProjectSource.ProjectDirectory, @"TestFiles\Bundles Mixed\");
    public static readonly string TestManyBundleFilesPath = Path.Combine(ProjectSource.ProjectDirectory, @"TestFiles\Bundles Mixed\VT2XZstd ALL 2023-11-21\");

    public static string? GetCompressionDict(string bundlePath)
    {
        if (!bundlePath.Contains("VT2XZstd"))
            return null;

        return Path.Combine(Path.GetDirectoryName(bundlePath)!, "compression.dictionary");
    }
}
