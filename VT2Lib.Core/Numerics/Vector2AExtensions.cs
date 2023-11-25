using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace VT2Lib.Core.Numerics;

public static class Vector2AExtensions
{
    public static Vector64<TNum> ToVector64<TNum>(this Vector2A<TNum> value)
        where TNum : unmanaged, IBinaryNumber<TNum>
    {
        if (Vector64<TNum>.Count != Vector2A<TNum>.Count)
            throw new ArgumentException($"{nameof(Vector64<TNum>)}.Count isn't equal to {nameof(Vector2A<TNum>)}.Count");

        return Unsafe.As<Vector2A<TNum>, Vector64<TNum>>(ref Unsafe.AsRef(in value));
    }

    public static Vector64<ushort> ToVector64(this Vector2A<Half> value)
    {
        return Unsafe.As<Vector2A<Half>, Vector2A<ushort>>(ref value).ToVector64();
    }

    public static Vector128<TNum> ToVector128<TNum>(this Vector2A<TNum> value)
        where TNum : unmanaged, IBinaryNumber<TNum>
    {
        if (Vector128<TNum>.Count != Vector2A<TNum>.Count)
            throw new ArgumentException($"{nameof(Vector128<TNum>)}.Count isn't equal to {nameof(Vector2A<TNum>)}.Count");

        return Unsafe.As<Vector2A<TNum>, Vector128<TNum>>(ref Unsafe.AsRef(in value));
    }

    public static Vector128<ushort> ToVector128(this Vector2A<Half> value)
    {
        return Unsafe.As<Vector2A<Half>, Vector2A<ushort>>(ref value).ToVector128();
    }

    public static TRootNum Length<TRootNum>(this scoped ref readonly Vector2A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        TRootNum lengthSquared = value.LengthSquared();
        return TRootNum.Sqrt(lengthSquared);
    }

    public static Span<TNum> AsSpan<TNum>(this scoped ref readonly Vector2A<TNum> value)
        where TNum : unmanaged, IBinaryNumber<TNum>
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in value.X), Vector2A<TNum>.Count);
    }

    public static Vector2 AsVector2(this scoped ref readonly Vector2A<float> value)
    {
        return Unsafe.ReadUnaligned<Vector2>(ref Unsafe.As<Vector2A<float>, byte>(ref Unsafe.AsRef(in value)));
    }
}