using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.Core.Numerics;

[StructLayout(LayoutKind.Sequential)]
public struct Vector2A<TNum> : IEquatable<Vector2A<TNum>>, IFormattable
    where TNum : unmanaged, IBinaryNumber<TNum>
{
    internal static int Size => Unsafe.SizeOf<TNum>() * Count;

    internal static int Count => 2;

    public static Vector2A<TNum> Zero { get; } = new(TNum.Zero);

    public static Vector2A<TNum> One { get; } = new(TNum.One);

    public static Vector2A<TNum> UnitX { get; } = new(TNum.One, TNum.Zero);

    public static Vector2A<TNum> UnitY { get; } = new(TNum.Zero, TNum.One);

    /// <summary>
    /// The X component of the vector.
    /// </summary>
    public TNum X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    public TNum Y;

    public Vector2A(TNum value)
        : this(value, value)
    {
    }

    public Vector2A(TNum x, TNum y)
    {
        X = x;
        Y = y;
    }

    public Vector2A(ReadOnlySpan<TNum> values)
    {
        if (values.Length < Count)
            throw new ArgumentOutOfRangeException(nameof(values), "Source span too small.");

        this = Unsafe.ReadUnaligned<Vector2A<TNum>>(ref Unsafe.As<TNum, byte>(ref MemoryMarshal.GetReference(values)));
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
        return (obj is Vector2A<TNum> other) && Equals(other);
    }

    public readonly bool Equals(Vector2A<TNum> other)
    {
        return X == other.X
            && Y == other.Y;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y);
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
        return $"<{X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> Abs(Vector2A<TNum> value)
    {
        return new Vector2A<TNum>(
            TNum.Abs(value.X),
            TNum.Abs(value.Y)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> Min(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return new Vector2A<TNum>(
            TNum.Min(left.X, right.X),
            TNum.Min(left.Y, right.Y)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> Max(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return new Vector2A<TNum>(
            TNum.Max(left.X, right.X),
            TNum.Max(left.Y, right.Y)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TNum Dot(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return (left.X * right.X) + (left.Y * right.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TRootNum> Sqrt<TRootNum>(Vector2A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        return new Vector2A<TRootNum>(
            TRootNum.Sqrt(value.X),
            TRootNum.Sqrt(value.Y)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TRootNum> Normalize<TRootNum>(Vector2A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        return value / value.Length();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return !(left == right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator +(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return new Vector2A<TNum>(left.X + right.X, left.Y + right.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator +(Vector2A<TNum> left, TNum right)
    {
        return new Vector2A<TNum>(left.X + right, left.Y + right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator -(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return new Vector2A<TNum>(left.X - right.X, left.Y - right.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator -(Vector2A<TNum> left, TNum right)
    {
        return new Vector2A<TNum>(left.X - right, left.Y - right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator *(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return new Vector2A<TNum>(left.X * right.X, left.Y * right.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator *(Vector2A<TNum> left, TNum right)
    {
        return new Vector2A<TNum>(left.X * right, left.Y * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator /(Vector2A<TNum> left, Vector2A<TNum> right)
    {
        return new Vector2A<TNum>(left.X / right.X, left.Y / right.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator /(Vector2A<TNum> left, TNum right)
    {
        return new Vector2A<TNum>(left.X / right, left.Y / right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2A<TNum> operator -(Vector2A<TNum> value)
    {
        return Zero - value;
    }
}