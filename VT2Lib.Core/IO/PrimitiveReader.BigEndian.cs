using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace VT2Lib.Core.IO;

public readonly ref partial struct PrimitiveReader
{
    public ushort ReadUInt16BE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    public short ReadInt16BE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadInt16BigEndian(buffer);
    }

    public uint ReadUInt32BE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    public int ReadInt32BE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    public ulong ReadUInt64BE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }

    public long ReadInt64BE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    public Half ReadHalfBE()
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Half>()];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadHalfBigEndian(buffer);
    }

    public float ReadSingleBE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadSingleBigEndian(buffer);
    }

    public double ReadDoubleBE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        ReadBytes(buffer);
        return BinaryPrimitives.ReadDoubleBigEndian(buffer);
    }

    public Vector2 ReadVector2BE()
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector2>()];
        ReadBytes(buffer);
        float x = BinaryPrimitives.ReadSingleBigEndian(buffer);
        float y = BinaryPrimitives.ReadSingleBigEndian(buffer[4..]);
        return new Vector2(x, y);
    }

    public Vector3 ReadVector3BE()
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector3>()];
        ReadBytes(buffer);
        float x = BinaryPrimitives.ReadSingleBigEndian(buffer);
        float y = BinaryPrimitives.ReadSingleBigEndian(buffer[4..]);
        float z = BinaryPrimitives.ReadSingleBigEndian(buffer[8..]);
        return new Vector3(x, y, z);
    }
    
    public Vector4 ReadVector4BE()
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector4>()];
        ReadBytes(buffer);
        float x = BinaryPrimitives.ReadSingleBigEndian(buffer);
        float y = BinaryPrimitives.ReadSingleBigEndian(buffer[4..]);
        float z = BinaryPrimitives.ReadSingleBigEndian(buffer[8..]);
        float w = BinaryPrimitives.ReadSingleBigEndian(buffer[12..]);
        return new Vector4(x, y, z, w);
    }

    public Matrix4x4 ReadMatrix3x3BE()
    {
        Span<byte> buffer = stackalloc byte[sizeof(float) * 9];
        ReadBytes(buffer);

        Span<byte> r1 = buffer.Slice(0, 12);
        float m11 = BinaryPrimitives.ReadSingleBigEndian(r1);
        float m12 = BinaryPrimitives.ReadSingleBigEndian(r1[4..]);
        float m13 = BinaryPrimitives.ReadSingleBigEndian(r1[8..]);

        Span<byte> r2 = buffer.Slice(12, 12);
        float m21 = BinaryPrimitives.ReadSingleBigEndian(r2);
        float m22 = BinaryPrimitives.ReadSingleBigEndian(r2[4..]);
        float m23 = BinaryPrimitives.ReadSingleBigEndian(r2[8..]);

        Span<byte> r3 = buffer.Slice(24, 12);
        float m31 = BinaryPrimitives.ReadSingleBigEndian(r3);
        float m32 = BinaryPrimitives.ReadSingleBigEndian(r3[4..]);
        float m33 = BinaryPrimitives.ReadSingleBigEndian(r3[8..]);

        return new Matrix4x4
        (
            m11, m12, m13, 0.0f,
            m21, m22, m23, 0.0f,
            m31, m32, m33, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );
    }

    public Matrix4x4 ReadMatrix4x4BE()
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Matrix4x4>()];
        ReadBytes(buffer);

        Span<byte> r1 = buffer.Slice(0, 16);
        float m11 = BinaryPrimitives.ReadSingleBigEndian(r1);
        float m12 = BinaryPrimitives.ReadSingleBigEndian(r1[4..]);
        float m13 = BinaryPrimitives.ReadSingleBigEndian(r1[8..]);
        float m14 = BinaryPrimitives.ReadSingleBigEndian(r1[12..]);

        Span<byte> r2 = buffer.Slice(16, 16);
        float m21 = BinaryPrimitives.ReadSingleBigEndian(r2);
        float m22 = BinaryPrimitives.ReadSingleBigEndian(r2[4..]);
        float m23 = BinaryPrimitives.ReadSingleBigEndian(r2[8..]);
        float m24 = BinaryPrimitives.ReadSingleBigEndian(r2[12..]);

        Span<byte> r3 = buffer.Slice(32, 16);
        float m31 = BinaryPrimitives.ReadSingleBigEndian(r3);
        float m32 = BinaryPrimitives.ReadSingleBigEndian(r3[4..]);
        float m33 = BinaryPrimitives.ReadSingleBigEndian(r3[8..]);
        float m34 = BinaryPrimitives.ReadSingleBigEndian(r3[12..]);

        Span<byte> r4 = buffer.Slice(48, 16);
        float m41 = BinaryPrimitives.ReadSingleBigEndian(r4);
        float m42 = BinaryPrimitives.ReadSingleBigEndian(r4[4..]);
        float m43 = BinaryPrimitives.ReadSingleBigEndian(r4[8..]);
        float m44 = BinaryPrimitives.ReadSingleBigEndian(r4[12..]);

        return new Matrix4x4
        (
            m11, m12, m13, m14,
            m21, m22, m23, m24,
            m31, m32, m33, m34,
            m41, m42, m43, m44
        );
    }

    public Quaternion ReadQuaternionBE()
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Quaternion>()];
        ReadBytes(buffer);
        float x = BinaryPrimitives.ReadSingleBigEndian(buffer);
        float y = BinaryPrimitives.ReadSingleBigEndian(buffer[4..]);
        float z = BinaryPrimitives.ReadSingleBigEndian(buffer[8..]);
        float w = BinaryPrimitives.ReadSingleBigEndian(buffer[12..]);

        return new Quaternion(x, y, z, w);
    }
}