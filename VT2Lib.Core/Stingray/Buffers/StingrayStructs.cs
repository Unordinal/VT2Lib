using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2Lib.Core.Stingray.Collections;

namespace VT2Lib.Core.Stingray.Buffers;


public static class StingrayStructs
{
    public static IDString32 ReadIDString32(ReadOnlySpan<byte> source, IIDString32Provider? idStringProvider = null)
    {
        if (source.Length < 4)
            ThrowSpanTooSmall(nameof(IDString32), nameof(source));

        idStringProvider ??= IDStringRepository.Shared;
        uint id = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(source));

        return idStringProvider.TryGet(id, out var idString)
            ? idString
            : new IDString32(id);
    }

    public static IDString64 ReadIDString64(ReadOnlySpan<byte> source, IIDString64Provider? idStringProvider = null)
    {
        if (source.Length < 8)
            ThrowSpanTooSmall(nameof(IDString64), nameof(source));

        idStringProvider ??= IDStringRepository.Shared;
        ulong id = Unsafe.ReadUnaligned<ulong>(ref MemoryMarshal.GetReference(source));

        return idStringProvider.TryGet(id, out var idString)
            ? idString
            : new IDString64(id);
    }

    public static ResourceLocator ReadResourceLocator(ReadOnlySpan<byte> source, IIDString64Provider? idStringProvider = null)
    {
        if (source.Length < 16)
            ThrowSpanTooSmall(nameof(ResourceLocator), nameof(source));

        idStringProvider ??= IDStringRepository.Shared;
        var type = ReadIDString64(source, idStringProvider);
        var name = ReadIDString64(source, idStringProvider);

        return new ResourceLocator(type, name);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowSpanTooSmall(string typeName, string? paramName)
    {
        throw new ArgumentOutOfRangeException(paramName, $"The source span is too small to contain a valid {typeName}.");
    }
}