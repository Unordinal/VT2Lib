namespace VT2Lib.Core.Stingray.Attributes;

// EVAL: Usage of an attribute vs a static virtual interface member.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class ResourceVersionAttribute : Attribute
{
    public const int Versionless = 0;

    public int Version { get; }

    public ResourceVersionAttribute(int version)
    {
        Version = version;
    }
}