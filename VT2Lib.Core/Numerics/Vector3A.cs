using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.Core.Numerics;

public static class Vector3A
{
    public const int Count = 3;

    public static TRootNum Length<TRootNum>(this ref readonly Vector3A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        TRootNum lengthSquared = value.LengthSquared();
        return TRootNum.Sqrt(lengthSquared);
    }

    public static Span<TNum> AsSpan<TNum>(this scoped ref readonly Vector3A<TNum> value)
        where TNum : unmanaged, IBinaryNumber<TNum>
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in value.X), Count);
    }

    public static Vector3 AsVector3(this scoped ref readonly Vector3A<float> value)
    {
        return Unsafe.ReadUnaligned<Vector3>(ref Unsafe.As<Vector3A<float>, byte>(ref Unsafe.AsRef(in value)));
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Vector3A<TNum> : IEquatable<Vector3A<TNum>>, IFormattable
    where TNum : unmanaged, IBinaryNumber<TNum>
{
    private const int Count = Vector3A.Count;

    public static int Size => Unsafe.SizeOf<TNum>() * Count;

    public static Vector3A<TNum> Zero { get; } = new(TNum.Zero);

    public static Vector3A<TNum> One { get; } = new(TNum.One);

    public static Vector3A<TNum> UnitX { get; } = new(TNum.One, TNum.Zero, TNum.Zero);

    public static Vector3A<TNum> UnitY { get; } = new(TNum.Zero, TNum.One, TNum.Zero);

    public static Vector3A<TNum> UnitZ { get; } = new(TNum.Zero, TNum.Zero, TNum.One);

    public TNum this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return Unsafe.Add(ref Unsafe.AsRef(in X), index);
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            Unsafe.Add(ref Unsafe.AsRef(in X), index) = value;
        }
    }

    /// <summary>
    /// The X component of the vector.
    /// </summary>
    public TNum X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    public TNum Y;

    /// <summary>
    /// The Z component of the vector.
    /// </summary>
    public TNum Z;

    public Vector3A(TNum value)
        : this(value, value, value)
    {
    }

    public Vector3A(TNum x, TNum y, TNum z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3A(ReadOnlySpan<TNum> values)
    {
        if (values.Length < Count)
            throw new ArgumentOutOfRangeException(nameof(values), "Source span too small.");

        this = Unsafe.ReadUnaligned<Vector3A<TNum>>(ref Unsafe.As<TNum, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public readonly TNum LengthSquared()
    {
        return Dot(this, this);
    }

    public readonly void CopyTo(Span<TNum> destination)
    {
        if (destination.Length < Count)
            throw new ArgumentException("The destination span is too short.");

        Unsafe.WriteUnaligned(ref Unsafe.As<TNum, byte>(ref MemoryMarshal.GetReference(destination)), this);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return (obj is Vector3A<TNum> other) && Equals(other);
    }

    public readonly bool Equals(Vector3A<TNum> other)
    {
        return X == other.X
            && Y == other.Y
            && Z == other.Z;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>Returns the string representation of the current instance using default formatting.</summary>
    /// <returns>The string representation of the current instance.</returns>
    /// <remarks>
    ///     This method returns a string in which each element of the vector is formatted using the "G" (general) format
    ///     string and the formatting conventions of the current thread culture. The "&lt;" and "&gt;" characters are used
    ///     to begin and end the string, and the current culture's <see cref="NumberFormatInfo.NumberGroupSeparator" /> property
    ///     followed by a space is used to separate each element.
    /// </remarks>
    public override readonly string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    /// <summary>Returns the string representation of the current instance using the specified format string to format individual elements.</summary>
    /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
    /// <returns>The string representation of the current instance.</returns>
    /// <remarks>
    ///     This method returns a string in which each element of the vector is formatted using <paramref name="format" /> and the
    ///     current culture's formatting conventions. The "&lt;" and "&gt;" characters are used to begin and end the string, and the
    ///     current culture's <see cref="NumberFormatInfo.NumberGroupSeparator" /> property followed by a space is used to separate
    ///     each element.
    /// </remarks>
    /// <related type="Article" href="/dotnet/standard/base-types/standard-numeric-format-strings">Standard Numeric Format Strings</related>
    /// <related type="Article" href="/dotnet/standard/base-types/custom-numeric-format-strings">Custom Numeric Format Strings</related>
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    /// <summary>
    ///     Returns the string representation of the current instance using the specified format string to format individual
    ///     elements and the specified format provider to define culture-specific formatting.
    /// </summary>
    /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
    /// <param name="formatProvider">A format provider that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the current instance.</returns>
    /// <remarks>
    ///     This method returns a string in which each element of the vector is formatted using <paramref name="format" />
    ///     and <paramref name="formatProvider" />. The "&lt;" and "&gt;" characters are used to begin and end the string,
    ///     and the format provider's <see cref="NumberFormatInfo.NumberGroupSeparator" /> property followed by a space is
    ///     used to separate each element.
    /// </remarks>
    /// <related type="Article" href="/dotnet/standard/base-types/custom-numeric-format-strings">Custom Numeric Format Strings</related>
    /// <related type="Article" href="/dotnet/standard/base-types/standard-numeric-format-strings">Standard Numeric Format Strings</related>
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        return
            $"<{X.ToString(format, formatProvider)}{separator} " +
            $"{Y.ToString(format, formatProvider)}{separator} " +
            $"{Z.ToString(format, formatProvider)}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> Abs(Vector3A<TNum> value)
    {
        return new Vector3A<TNum>(
            TNum.Abs(value.X),
            TNum.Abs(value.Y),
            TNum.Abs(value.Z)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> Min(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return new Vector3A<TNum>(
            TNum.Min(left.X, right.X),
            TNum.Min(left.Y, right.Y),
            TNum.Min(left.Z, right.Z)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> Max(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return new Vector3A<TNum>(
            TNum.Max(left.X, right.X),
            TNum.Max(left.Y, right.Y),
            TNum.Max(left.Z, right.Z)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TNum Dot(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return
            (left.X * right.X) +
            (left.Y * right.Y) +
            (left.Z * right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TRootNum> Sqrt<TRootNum>(Vector3A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        return new Vector3A<TRootNum>(
            TRootNum.Sqrt(value.X),
            TRootNum.Sqrt(value.Y),
            TRootNum.Sqrt(value.Z)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TRootNum> Normalize<TRootNum>(Vector3A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        return value / value.Length();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return !(left == right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator +(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return new Vector3A<TNum>(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator +(Vector3A<TNum> left, TNum right)
    {
        return new Vector3A<TNum>(
            left.X + right,
            left.Y + right,
            left.Z + right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator -(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return new Vector3A<TNum>(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator -(Vector3A<TNum> left, TNum right)
    {
        return new Vector3A<TNum>(
            left.X - right,
            left.Y - right,
            left.Z - right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator *(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return new Vector3A<TNum>(
            left.X * right.X,
            left.Y * right.Y,
            left.Z * right.Z
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator *(Vector3A<TNum> left, TNum right)
    {
        return new Vector3A<TNum>(
            left.X * right,
            left.Y * right,
            left.Z * right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator /(Vector3A<TNum> left, Vector3A<TNum> right)
    {
        return new Vector3A<TNum>(
            left.X / right.X,
            left.Y / right.Y,
            left.Z / right.Z
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator /(Vector3A<TNum> left, TNum right)
    {
        return new Vector3A<TNum>(
            left.X / right,
            left.Y / right,
            left.Z / right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3A<TNum> operator -(Vector3A<TNum> value)
    {
        return Zero - value;
    }
}