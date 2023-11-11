using VT2Lib.Core.Stingray;

namespace VT2Lib.Bundles.Resources;

// EVAL: Should this be a struct? Do we care?
public sealed record class BundledResourceMeta(ResourceLocator ResourceLocator, BundledResourceFlag Flag, uint TotalSize)
{
    public static int GetSizeForBundleVersion(BundleVersion version)
    {
        // TODO: VT1 is very likely wrong and VT2 is possibly wrong. Low priority, but check against each bundle version.
        return version switch
        {
            BundleVersion.VT1 => 20,
            BundleVersion.VT2 => 20,
            BundleVersion.VT2X => 24,
            _ => throw new ArgumentException($"Invalid bundle version '{version}'")
        };
    }
}