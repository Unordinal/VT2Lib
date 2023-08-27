using VT2Lib.Bundles.Resources;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Extensions;

namespace VT2Lib.Bundles.Extensions;

internal static class PrimitiveReaderExtensions
{
    public static BundledResourceMeta ReadBundledResourceMeta(this PrimitiveReader reader, IIDString64Provider? idStringProvider = null)
    {
        idStringProvider ??= IDStringRepository.Shared;

        // TODO: Check compat. with bundle versions < VT2X. Old versions are likely missing the TotalSize field.
        // (And maybe the flag field too?)
        // Use BundledResourceMeta.GetSizeForBundleVersion() and pass the version in.
        return new BundledResourceMeta
        (
            reader.ReadResourceLocator(idStringProvider),
            (BundledResourceFlag)reader.ReadUInt32LE(),
            reader.ReadUInt32LE()
        );
    }

    public static BundledResource ReadBundledResource(this PrimitiveReader reader, IIDStringProvider? idStringProvider = null)
    {
        idStringProvider ??= IDStringRepository.Shared;
        var resourceLocator = reader.ReadResourceLocator(idStringProvider);
        uint variantCount = reader.ReadUInt32LE();
        uint streamOffset = reader.ReadUInt32LE();

        var variantsMeta = new BundledResourceVariantMeta[variantCount];
        for (int i = 0; i < variantCount; i++)
            variantsMeta[i] = BundledResourceVariantMeta.Read(reader);

        var variantsData = new byte[variantCount][];
        for (int i = 0; i < variantCount; i++)
            variantsData[i] = reader.ReadBytes((int)variantsMeta[i].Size);

        return new BundledResource
        {
            ResourceLocator = resourceLocator,
            VariantCount = variantCount,
            StreamOffset = streamOffset,
            VariantsMeta = variantsMeta,
            VariantsData = variantsData
        };
    }

    public static BundledResourceVariantMeta ReadBundledResourceVariantMeta(this PrimitiveReader reader)
    {
        return new BundledResourceVariantMeta
        (
            (ResourceLanguage)reader.ReadUInt32LE(),
            reader.ReadUInt32LE(),
            reader.ReadUInt32LE()
        );
    }
}