using System.Diagnostics;
using System.Text;

namespace VT2Lib.Core.Extensions;

public static class BinaryWriterExtensions
{
    public static void WriteNullTermString(this BinaryWriter writer, string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using StackAllocHelper<byte> buffer = value.Length <= 256
            ? new(stackalloc byte[value.Length])
            : new(value.Length);

        int bytesWritten = Encoding.ASCII.GetBytes(value, buffer.Span);
        Debug.Assert(bytesWritten == value.Length);

        writer.Write(buffer.Span);
    }
}