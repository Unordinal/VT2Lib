using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.Core.Numerics;

public static class Vector4A
{
    public const int Count = 4;

    public static TRootNum Length<TRootNum>(this ref readonly Vector4A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        TRootNum lengthSquared = value.LengthSquared();
        return TRootNum.Sqrt(lengthSquared);
    }

    public static Span<TNum> AsSpan<TNum>(this scoped ref readonly Vector4A<TNum> value)
        where TNum : unmanaged, IBinaryNumber<TNum>
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in value.X), Count);
    }

    public static Vector2 AsVector2(this scoped ref readonly Vector4A<float> value)
    {
        return Unsafe.ReadUnaligned<Vector2>(ref UnsafeEx.GetByteRef(in value));
    }

    public static Vector3 AsVector3(this scoped ref readonly Vector4A<float> value)
    {
        return Unsafe.ReadUnaligned<Vector3>(ref UnsafeEx.GetByteRef(in value));
    }

    public static Vector4 AsVector4(this scoped ref readonly Vector4A<float> value)
    {
        return Unsafe.ReadUnaligned<Vector4>(ref UnsafeEx.GetByteRef(in value));
    }

    public static Quaternion AsQuaternion(this scoped ref readonly Vector4A<float> value)
    {
        return Unsafe.ReadUnaligned<Quaternion>(ref Unsafe.As<Vector4A<float>, byte>(ref Unsafe.AsRef(in value)));
    }

    public static Vector4A<TTo> ReinterpretCast<TFrom, TTo>(this scoped ref readonly Vector4A<TFrom> value)
        where TFrom : unmanaged, IBinaryNumber<TFrom>
        where TTo : unmanaged, IBinaryNumber<TTo>
    {
        int sizeOfTFrom = Unsafe.SizeOf<TFrom>();
        int sizeOfTTo = Unsafe.SizeOf<TTo>();
        if (sizeOfTFrom == sizeOfTTo)
        {
            return Unsafe.ReadUnaligned<Vector4A<TTo>>(ref Unsafe.As<TFrom, byte>(ref Unsafe.AsRef(in value.X))); // Unsafe.Bitcast would work but it does a type size check that we already do anyway
        }
        else if (sizeOfTFrom > sizeOfTTo)
        {
            ref byte pVec = ref Unsafe.As<TFrom, byte>(ref Unsafe.AsRef(in value.X));
            return new Vector4A<TTo>(
                Unsafe.ReadUnaligned<TTo>(ref pVec),
                Unsafe.ReadUnaligned<TTo>(ref Unsafe.AddByteOffset(ref pVec, sizeOfTFrom)),
                Unsafe.ReadUnaligned<TTo>(ref Unsafe.AddByteOffset(ref pVec, sizeOfTFrom * 2)),
                Unsafe.ReadUnaligned<TTo>(ref Unsafe.AddByteOffset(ref pVec, sizeOfTFrom * 3))
            );
        }
        else
        {
            Vector4A<TTo> newVec = default;
            ref byte pVec = ref Unsafe.As<TTo, byte>(ref newVec.X);
            Unsafe.WriteUnaligned(ref pVec, value.X);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref pVec, sizeOfTTo), value.Y);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref pVec, sizeOfTTo * 2), value.Z);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref pVec, sizeOfTTo * 3), value.W);

            return newVec;
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Vector4A<TNum> : IEquatable<Vector4A<TNum>>, IFormattable
    where TNum : unmanaged, IBinaryNumber<TNum>
{
    private const int Count = Vector4A.Count;

    public static int Size => Unsafe.SizeOf<TNum>() * Count;

    public static Vector4A<TNum> Zero { get; } = new(TNum.Zero);

    public static Vector4A<TNum> One { get; } = new(TNum.One);

    public static Vector4A<TNum> UnitX { get; } = new(TNum.One, TNum.Zero, TNum.Zero, TNum.Zero);

    public static Vector4A<TNum> UnitY { get; } = new(TNum.Zero, TNum.One, TNum.Zero, TNum.Zero);

    public static Vector4A<TNum> UnitZ { get; } = new(TNum.Zero, TNum.Zero, TNum.One, TNum.Zero);

    public static Vector4A<TNum> UnitW { get; } = new(TNum.Zero, TNum.Zero, TNum.Zero, TNum.One);

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

    /// <summary>
    /// The W component of the vector.
    /// </summary>
    public TNum W;

    public Vector4A(TNum value)
        : this(value, value, value, value)
    {
    }

    public Vector4A(TNum x, TNum y, TNum z, TNum w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vector4A(ReadOnlySpan<TNum> values)
    {
        if (values.Length < Count)
            throw new ArgumentOutOfRangeException(nameof(values), "Source span too small.");

        this = Unsafe.ReadUnaligned<Vector4A<TNum>>(ref Unsafe.As<TNum, byte>(ref MemoryMarshal.GetReference(values)));
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
        return (obj is Vector4A<TNum> other) && Equals(other);
    }

    public readonly bool Equals(Vector4A<TNum> other)
    {
        return X == other.X
            && Y == other.Y
            && Z == other.Z
            && W == other.W;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
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
            $"{Z.ToString(format, formatProvider)}{separator} " +
            $"{W.ToString(format, formatProvider)}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> Abs(Vector4A<TNum> value)
    {
        return new Vector4A<TNum>(
            TNum.Abs(value.X),
            TNum.Abs(value.Y),
            TNum.Abs(value.Z),
            TNum.Abs(value.W)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> Min(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return new Vector4A<TNum>(
            TNum.Min(left.X, right.X),
            TNum.Min(left.Y, right.Y),
            TNum.Min(left.Z, right.Z),
            TNum.Min(left.W, right.W)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> Max(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return new Vector4A<TNum>(
            TNum.Max(left.X, right.X),
            TNum.Max(left.Y, right.Y),
            TNum.Max(left.Z, right.Z),
            TNum.Max(left.W, right.W)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TNum Dot(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return
            (left.X * right.X) +
            (left.Y * right.Y) +
            (left.Z * right.Z) +
            (left.W * right.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TRootNum> Sqrt<TRootNum>(Vector4A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        return new Vector4A<TRootNum>(
            TRootNum.Sqrt(value.X),
            TRootNum.Sqrt(value.Y),
            TRootNum.Sqrt(value.Z),
            TRootNum.Sqrt(value.W)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TRootNum> Normalize<TRootNum>(Vector4A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        return value / value.Length();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return !(left == right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator +(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return new Vector4A<TNum>(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z,
            left.W + right.W
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator +(Vector4A<TNum> left, TNum right)
    {
        return new Vector4A<TNum>(
            left.X + right,
            left.Y + right,
            left.Z + right,
            left.W + right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator -(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return new Vector4A<TNum>(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z,
            left.W - right.W
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator -(Vector4A<TNum> left, TNum right)
    {
        return new Vector4A<TNum>(
            left.X - right,
            left.Y - right,
            left.Z - right,
            left.W - right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator *(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return new Vector4A<TNum>(
            left.X * right.X,
            left.Y * right.Y,
            left.Z * right.Z,
            left.W * right.W
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator *(Vector4A<TNum> left, TNum right)
    {
        return new Vector4A<TNum>(
            left.X * right,
            left.Y * right,
            left.Z * right,
            left.W * right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator /(Vector4A<TNum> left, Vector4A<TNum> right)
    {
        return new Vector4A<TNum>(
            left.X / right.X,
            left.Y / right.Y,
            left.Z / right.Z,
            left.W / right.W
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator /(Vector4A<TNum> left, TNum right)
    {
        return new Vector4A<TNum>(
            left.X / right,
            left.Y / right,
            left.Z / right,
            left.W / right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4A<TNum> operator -(Vector4A<TNum> value)
    {
        return Zero - value;
    }
}