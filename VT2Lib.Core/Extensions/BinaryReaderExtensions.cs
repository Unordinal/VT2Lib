using VT2Lib.Core.Collections;

namespace VT2Lib.Core.Extensions;

public static class BinaryReaderExtensions
{
    public static string ReadNullTermString(this BinaryReader reader)
    {
        using ValueList<char> list = new(stackalloc char[512]);
        byte b;
        while ((b = reader.ReadByte()) > 0)
            list.Add((char)b);

        return new string(list.AsSpan());
    }
}