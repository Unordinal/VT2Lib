using VT2Lib.Core.Stingray;

namespace VT2Lib.Bundles.Resources;

// TODO: We need a better name for this. This isn't really a singular resource (IResource). It usually does
// contain just a single resource, but in cases such as localized files, it can contain multiple resources of
// localized data (ex: strings_en.txt, strings_es.txt, strings_de.txt) (Not real files or file names, just an example.)
// Each variant in this is an IResource on its own, this is just a 'package'. Using that name, however, would conflict
// with Stingray's usage of the word.
internal sealed class BundledResource
{
    public ResourceLocator ResourceLocator { get; init; }

    public uint VariantCount { get; init; }

    public uint StreamOffset { get; init; }

    public BundledResourceVariantMeta[] VariantsMeta { get; init; } = Array.Empty<BundledResourceVariantMeta>();

    public byte[][] VariantsData { get; init; } = Array.Empty<byte[]>();
}