namespace VT2Lib.Core.Stingray.Attributes;

// EVAL: Add 'ResourceID'? Would make it possible to 'GetSerializer<TResource>()'
// for ResourceSerializerProvider without passing in the IDString64 manually.
// Could also just use reflection for the curent 'ID' constant.
// EVAL: Usage of an attribute vs a static virtual interface member.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class StingrayResourceAttribute : Attribute
{
    public const int Versionless = 0;

    private static IEnumerable<(Type, StingrayResourceAttribute)> AllWithAttr { get; } = ReflectionUtil.GetAllWithAttribute<StingrayResourceAttribute>(false);

    public int Version { get; }

    public StingrayResourceAttribute()
        : this(Versionless)
    {
    }

    public StingrayResourceAttribute(int version)
    {
        Version = version;
    }
}