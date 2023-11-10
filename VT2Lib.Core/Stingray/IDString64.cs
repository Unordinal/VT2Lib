using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2Lib.Core.Stingray.Hashing;

namespace VT2Lib.Core.Stingray;

/// <summary>
/// Represents a 64-bit Murmur-hashed string value.
/// </summary>
/// <remarks>Reference: <see href="https://github.com/AutodeskGames/stingray-plugin/blob/master/stingray_sdk/plugin_foundation/id_string.h">id_string.h</see></remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct IDString64 : IComparable<IDString64>
{
    /// <summary>
    /// Gets the empty string.
    /// </summary>
    public static IDString64 Empty { get; } = new(0, string.Empty);

    /// <summary>
    /// Gets the 64-bit hash of the original string value.
    /// </summary>
    public ulong ID { get; }

    /// <summary>
    /// Gets the original string value, if we know what it is.
    /// <para/>
    /// Consider using <see cref="ToString"/> instead of <c>Value ?? ID.ToString("x16")</c>.
    /// </summary>
    /// <returns>The original string value if we know it; otherwise, <see langword="null"/>.</returns>
    public string? Value { get; }

    /// <summary>
    /// Creates a new <see cref="IDString64"/> using the specified hash and without a known string value.
    /// </summary>
    /// <param name="id">The hash of the string.</param>
    public IDString64(ulong id)
    {
        ID = id;
        Value = null;
    }

    /// <summary>
    /// Creates a new <see cref="IDString64"/> using the specified string and hashing it to create the ID.
    /// </summary>
    /// <param name="value">The string value of the ID.</param>
    public IDString64(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        ID = Murmur.Hash64(value);
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="IDString64"/> using the specified hash and string value.
    /// </summary>
    /// <remarks>The string hash is not validated for correctness unless compiled in 'Debug' mode and <c>VALIDATE_IDSTRING_HASHES</c> is defined.</remarks>
    /// <param name="id">The murmur hash of the string.</param>
    /// <param name="value">The string value of the ID.</param>
    public IDString64(ulong id, string? value)
    {
#if VALIDATE_IDSTRING_HASHES
        Debug.Assert(value is null || id == Murmur.Hash64(value), $"The specified ID {id:x16} does not match the 64-bit hash of the string '{value}'. ({id:x16} != {Murmur.Hash64(value!):x16})");
#endif
        ID = id;
        Value = value;
    }

    /// <summary>
    ///     Gets the string representation of this <see cref="IDString64"/>.
    /// </summary>
    /// <returns>
    ///     If <see cref="Value"/> is not <see langword="null"/>, returns <see cref="Value"/>;
    ///     otherwise, returns the result of <see cref="ToIdentifier"/>.
    /// </returns>
    public override string ToString()
    {
        return Value ?? ToIdentifier();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(IDString64 other)
    {
        return ID == other.ID;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(IDString64 other)
    {
        return ID.CompareTo(other.ID);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    /// <summary>
    /// Returns the ID of this <see cref="IDString64"/> as a string in the format <c>#ID[0123456789abcdef]</c>.
    /// </summary>
    /// <remarks>This is the way the Stingray engine formats its identifier strings.</remarks>
    /// <returns>The formatted ID of this <see cref="IDString64"/>.</returns>
    public string ToIdentifier()
    {
        return $"#ID[{ID:x16}]";
    }

    public static bool operator <(IDString64 left, IDString64 right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(IDString64 left, IDString64 right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(IDString64 left, IDString64 right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(IDString64 left, IDString64 right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static implicit operator IDString64(string value) => new(value);

    /// <summary>
    /// Gets whether the specified string is an empty string (<see cref="ID"/> == 0).
    /// </summary>
    /// <returns><see langword="true"/> if <paramref name="value"/> is an empty string (<see cref="ID"/> == 0); otherwise, <see langword="false"/>.</returns>
    public static bool IsEmpty(IDString64 value)
    {
        return value.ID == 0;
    }
}