using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.Core.Numerics;

public static class Vector4AExtensions
{
    public static TRootNum Length<TRootNum>(this ref readonly Vector4A<TRootNum> value)
        where TRootNum : unmanaged, IBinaryNumber<TRootNum>, IRootFunctions<TRootNum>
    {
        TRootNum lengthSquared = value.LengthSquared();
        return TRootNum.Sqrt(lengthSquared);
    }

    public static Span<TNum> AsSpan<TNum>(this scoped ref readonly Vector4A<TNum> value)
        where TNum : unmanaged, IBinaryNumber<TNum>
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in value.X), Vector4A<TNum>.Count);
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