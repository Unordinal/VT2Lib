namespace VT2Lib.Bundles;

/// <summary>
/// Represents a Vermintide bundle version.
/// </summary>
public enum BundleVersion : uint
{
    /// <summary>
    /// The base bundle version. Used in Vermintide 1.
    /// </summary>
    VT1 = 0xF0000004,
    /// <summary>
    /// The flags bundle version. Used in Vermintide 2. Presumably the version in which the resource patch flags were added.
    /// </summary>
    VT2 = 0xF0000005,
    /// <summary>
    /// The size bundle version. Used in Vermintide 2.X. Presumably the version in which each resource's size was added to the information in the resource list.
    /// </summary>
    VT2X = 0xF0000006
}