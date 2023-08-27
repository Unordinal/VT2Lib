using System.Reflection;

namespace VT2Lib.Tests.Attributes;

/// <summary>
/// Works like <see cref="MemberDataAttribute"/> but allows you to specify further parameters that 
/// will be concat'd to each object[] returned by the member. Makes it easier to do permutations on top of data.
/// </summary>
internal sealed class MemberDataWithInlineAttribute : MemberDataAttributeBase
{
    private readonly object[] _data;

    public MemberDataWithInlineAttribute(string memberName, params object[] additionalData) : this(memberName, Array.Empty<object>(), additionalData)
    {

    }

    public MemberDataWithInlineAttribute(string memberName, object[] memberParameters, params object[] additionalData) : base(memberName, memberParameters)
    {
        _data = additionalData;
    }

    protected override object[]? ConvertDataItem(MethodInfo testMethod, object item)
    {
        if (item == null)
            return null;

        var array = item as object[];
        if (array == null)
            throw new ArgumentException($"Property {MemberName} on {MemberType ?? testMethod.DeclaringType} yielded an item that is not an object[]");

        object[] data = array.Concat(_data).ToArray();
        return data;
    }
}