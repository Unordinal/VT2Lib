using System.Runtime.CompilerServices;

namespace VT2Lib.Core.IO;

/// <summary>
/// Indicates the endianness, or byte order, of some data.
/// </summary>
public enum ByteOrder
{
    /// <summary>
    /// The data is in little endian byte order, with the least-significant bytes at the start.
    /// </summary>
    LittleEndian,

    /// <summary>
    /// The data is in big endian byte order, with the least-significant bytes at the end.
    /// </summary>
    BigEndian
}

public static class ByteOrderUtil
{
    /// <summary>
    /// Gets the endianness of this machine.
    /// </summary>
    public static ByteOrder SystemEndianness { get; } = BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian;

    /// <summary>
    /// Gets whether the current machine is little endian.
    /// </summary>
    /// <returns><see langword="true"/> if the current machine architecture stores data in little endian byte order; otherwise, <see langword="false"/>.</returns>
    public static bool IsLitteEndian => BitConverter.IsLittleEndian;

    /// <summary>
    /// Returns whether the specified endianness value is equal to the current machine's endianness.
    /// </summary>
    /// <param name="endianness"></param>
    /// <returns></returns>
    public static bool EqualsSystemEndianness(this ByteOrder endianness)
    {
        return endianness == SystemEndianness;
    }

    /// <summary>
    /// Returns whether the specified endianness value is valid.
    /// </summary>
    /// <param name="endianness">The endianness value.</param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="endianness"/> is equal to <see cref="ByteOrder.LittleEndian"/> or <see cref="ByteOrder.BigEndian"/>;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValidByteOrder(this ByteOrder endianness)
    {
        return endianness is ByteOrder.LittleEndian or ByteOrder.BigEndian;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified <paramref name="endianness"/> value is
    /// not equal to <see cref="ByteOrder.LittleEndian"/> or <see cref="ByteOrder.BigEndian"/>.
    /// </summary>
    /// <param name="endianness"></param>
    /// <param name="paramName"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void ThrowIfInvalid(this ByteOrder endianness, [CallerArgumentExpression(nameof(endianness))] string? paramName = null)
    {
        if (!endianness.IsValidByteOrder())
            throw new ArgumentOutOfRangeException(paramName, $"The specified byte order value '{endianness}' is invalid.");
    }
}