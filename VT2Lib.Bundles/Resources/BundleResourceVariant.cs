﻿using System.Diagnostics;

namespace VT2Lib.Bundles.Resources;

// Public facade for BundledResourceVariant.
// TODO: come up with a better name, god
public sealed class BundleResourceVariant
{
    public BundleResource ParentResource { get; }

    public ResourceLanguage Language => _variantMeta.Language;

    public uint Size => _variantMeta.Size;

    public uint StreamSize => _variantMeta.StreamSize;

    public ReadOnlySpan<byte> Data => _variantData;

    private readonly BundledResourceVariantMeta _variantMeta;
    private readonly byte[] _variantData;

    internal BundleResourceVariant(BundleResource parentResource, BundledResourceVariantMeta variantMeta, byte[] variantData)
    {
        Debug.Assert(variantMeta is not null);
        Debug.Assert(variantData is not null);

        ParentResource = parentResource;
        _variantMeta = variantMeta;
        _variantData = variantData;
    }
}