﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2Lib.Core.Stingray.Collections;

namespace VT2Lib.Core.Stingray;

/// <summary>
/// Represents a 32-bit Murmur-hashed string value.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct IDString32 : IEquatable<IDString32>, IComparable<IDString32>
{
    /// <summary>
    /// Gets the empty string.
    /// </summary>
    public static IDString32 Empty { get; } = new(0, string.Empty);

    /// <summary>
    /// Gets whether this is the empty string.
    /// </summary>
    /// <returns><see langword="true"/> if this is the empty string (<see cref="ID"/> == 0); otherwise, <see langword="false"/>.</returns>
    public bool IsEmpty => ID == 0;

    /// <summary>
    /// Gets the 32-bit hash of the original string value.
    /// </summary>
    public uint ID { get; }

    /// <summary>
    /// Gets the original string value, if we know what it is.
    /// <para/>
    /// Consider using <see cref="ToString"/> instead of <c>Value ?? ID.ToString("x8")</c>.
    /// </summary>
    /// <returns>The original string value if we know it; otherwise, <see langword="null"/>.</returns>
    public string? Value { get; }

    /// <summary>
    /// Creates a new <see cref="IDString32"/> using the specified hash and without a known string value.
    /// </summary>
    /// <param name="id">The hash of the string.</param>
    public IDString32(uint id)
    {
        ID = id;
        Value = null;
    }

    /// <summary>
    /// Creates a new <see cref="IDString32"/> using the specified string and hashing it to create the ID.
    /// </summary>
    /// <param name="value">The string value of the ID.</param>
    public IDString32(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        ID = Murmur.Hash32(value);
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="IDString32"/> using the specified hash and string value.
    /// </summary>
    /// <remarks>The hash is not validated for correctness unless compiled in 'Debug' and <c>VALIDATE_IDSTRING_HASHES</c> is set.</remarks>
    /// <param name="id">The hash of the string.</param>
    /// <param name="value">The string value of the ID.</param>
    public IDString32(uint id, string? value)
    {
#if VALIDATE_IDSTRING_HASHES
        Debug.Assert(value is null || id == Murmur.Hash32(value), $"The specified ID {id:x8} does not match the 32-bit hash of the string '{value}'. ({id:x8} != {Murmur.Hash32(value!):x8})");
#endif
        ID = id;
        Value = value;
    }

    /// <summary>
    ///     Gets the string representation of this <see cref="IDString32"/>.
    /// </summary>
    /// <returns>
    ///     If <see cref="Value"/> is not <see langword="null"/>, returns <see cref="Value"/>;
    ///     otherwise, returns <see cref="ID"/> as a 32-bit hexadecimal string.
    /// </returns>
    public override string ToString()
    {
        return Value ?? ID.ToString("x8");
    }

    /// <summary>
    /// Gets the string representation of this <see cref="IDString32"/>, using the given <see cref="IDStringRepository"/> to lookup the original string value.
    /// </summary>
    /// <param name="idStringRepo">The repository to look up the original string value in.</param>
    /// <returns>
    ///     If <see cref="ID"/> is found within <paramref name="idStringRepo"/>, returns the result of calling <see cref="ToString"/> on the found value;
    ///     otherwise, returns the result of calling <see cref="ToString"/> on this instance.
    /// </returns>
    public string ToString(IDStringRepository? idStringRepo)
    {
        idStringRepo ??= IDStringRepository.Shared;
        if (idStringRepo.TryGet(ID, out var foundValue))
            return foundValue.ToString();

        return ToString();
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is IDString32 idString && Equals(idString);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(IDString32 other)
    {
        return ID == other.ID;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(IDString32 other)
    {
        return ID.CompareTo(other.ID);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    /// <summary>
    /// Returns the ID of this <see cref="IDString32"/> as a string in the format <c>#ID[0123abcd]</c>.
    /// </summary>
    /// <remarks>This is the way the Stingray engine formats its identifier strings.</remarks>
    /// <returns>The formatted ID of this <see cref="IDString32"/>.</returns>
    public string ToIdentifier()
    {
        return $"#ID[{ID:x8}]";
    }

    public static bool operator ==(IDString32 left, IDString32 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(IDString32 left, IDString32 right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(IDString32 left, IDString32 right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(IDString32 left, IDString32 right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(IDString32 left, IDString32 right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(IDString32 left, IDString32 right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static implicit operator IDString32(string value) => new(value);
}