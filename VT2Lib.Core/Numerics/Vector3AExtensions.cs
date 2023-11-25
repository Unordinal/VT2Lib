using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.Core.Numerics;

public static class Vector3AExtensions
{
    public static TRootNum Length<TRootNum>(this ref readonly Vector3A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        TRootNum lengthSquared = value.LengthSquared();
        return TRootNum.Sqrt(lengthSquared);
    }

    public static Span<TNum> AsSpan<TNum>(this scoped ref readonly Vector3A<TNum> value)
        where TNum : unmanaged, IBinaryNumber<TNum>
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in value.X), Vector3A<TNum>.Count);
    }

    public static Vector3 AsVector3(this scoped ref readonly Vector3A<float> value)
    {
        return Unsafe.ReadUnaligned<Vector3>(ref Unsafe.As<Vector3A<float>, byte>(ref Unsafe.AsRef(in value)));
    }
}