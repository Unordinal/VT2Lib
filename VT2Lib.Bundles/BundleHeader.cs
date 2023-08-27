using VT2Lib.Bundles.Resources;
using VT2Lib.Core.Stingray;

namespace VT2Lib.Bundles;

public sealed class BundleHeader
{
    /// <summary>
    /// Get the bundle's version.
    /// </summary>
    public required BundleVersion Version { get; init; }

    /// <summary>
    /// Gets the uncompressed size of the bundle.
    /// </summary>
    public required long Size { get; init; }

    /// <summary>
    /// Gets whether the bundle is compressed.
    /// </summary>
    public required bool IsCompressed { get; init; }

    /// <summary>
    /// Gets the number of resources in the bundle.
    /// </summary>
    public required int ResourceCount { get; init; }

    /// <summary>
    /// Gets the bundle's properties.
    /// </summary>
    public required IReadOnlyList<IDString64> Properties { get; init; }

    /// <summary>
    /// Gets a list of metadata about each resource in the bundle.
    /// </summary>
    public required IReadOnlyList<BundledResourceMeta> ResourceList { get; init; }

    internal BundleHeader()
    {

    }
}