using System.Reflection;

namespace VT2Lib.Core.Stingray.Attributes;

// EVAL: Usage of an attribute vs a static virtual interface member.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class StingrayResourceAttribute : Attribute
{
    public const int Versionless = 0;

    private static IEnumerable<(Type, StingrayResourceAttribute)> AllWithAttr { get; } = ReflectionUtil.GetAllWithAttribute<StingrayResourceAttribute>(false);

    public int Version { get; }

    public StingrayResourceAttribute(int version)
    {
        Version = version;
    }
}