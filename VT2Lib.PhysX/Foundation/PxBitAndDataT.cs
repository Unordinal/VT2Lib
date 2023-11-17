using System.Numerics;
using System.Runtime.InteropServices;

namespace VT2Lib.PhysX.Foundation;

[StructLayout(LayoutKind.Sequential)]
internal struct PxBitAndDataT<T>
    where T : unmanaged, IBinaryNumber<T>, IBitwiseOperators<T, T, T>
{
    private static T Bitmask => T.CopySign(T.AllBitsSet, T.Zero);

    public readonly T Value => _value & ~Bitmask;

    public readonly bool Bit => (_value & Bitmask) == Bitmask;

    private T _value;

    public PxBitAndDataT(T value)
    {
        _value = value;
    }

    public PxBitAndDataT(T value, bool bit)
    {
        _value = bit ? (value | Bitmask) : value;
    }

    public void SetBit()
    {
        _value |= Bitmask;
    }

    public void ClearBit()
    {
        _value &= ~Bitmask;
    }

    public static implicit operator PxBitAndDataT<T>(T value) => new(value);

    public static implicit operator T(PxBitAndDataT<T> value) => value.Value;
}