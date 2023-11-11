using System.Diagnostics;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.Stingray;

namespace VT2Lib.Bundles.Resources;

// Public facade over BundledResource.
// TODO: come up with a better name, god
public sealed class BundleResource
{
    public BundleResourceVariant this[int index] => Variants[index];

    public ResourceLocator ResourceLocator => _bundledResource.ResourceLocator;

    public uint VariantCount => _bundledResource.VariantCount;

    public uint StreamOffset => _bundledResource.StreamOffset;

    public IReadOnlyList<BundleResourceVariant> Variants => _variants.AsReadOnlyEx();

    private readonly BundledResource _bundledResource;
    private readonly BundleResourceVariant[] _variants;

    internal BundleResource(BundledResource bundledResource)
    {
        Debug.Assert(bundledResource is not null);
        _bundledResource = bundledResource;

        _variants = new BundleResourceVariant[_bundledResource.VariantCount];
        for (int i = 0; i < _bundledResource.VariantCount; i++)
            _variants[i] = new BundleResourceVariant(this, _bundledResource.VariantsMeta[i], _bundledResource.VariantsData[i]);
    }
}