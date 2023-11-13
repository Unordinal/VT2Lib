using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Attributes;

namespace VT2Lib.Core.Stingray.Resources.Unit;

[StingrayResource(186)]
public sealed class UnitResourceV186 : UnitResource
{
    public static UnitResourceV186 ReadBinary(in PrimitiveReader reader)
    {
        return new UnitResourceV186
        {
        };
    }

    public static void WriteBinary(in PrimitiveWriter writer, UnitResourceV186 resource)
    {
        throw new NotImplementedException();
    }
}