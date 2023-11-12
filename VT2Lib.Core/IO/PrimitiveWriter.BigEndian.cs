using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace VT2Lib.Core.IO;

public readonly ref partial struct PrimitiveWriter
{
    public void WriteUInt16BE(ushort value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteInt16BE(short value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteUInt32BE(uint value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteInt32BE(int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteUInt64BE(ulong value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteInt64BE(long value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteHalfBE(Half value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Half>()];
        BinaryPrimitives.WriteHalfBigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteSingleBE(float value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleBigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteDoubleBE(double value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteVector2BE(Vector2 value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector2>()];
        BinaryPrimitives.WriteSingleBigEndian(buffer, value.X);
        BinaryPrimitives.WriteSingleBigEndian(buffer[4..], value.Y);
        WriteBytes(buffer);
    }

    public void WriteVector3BE(Vector3 value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector3>()];
        BinaryPrimitives.WriteSingleBigEndian(buffer, value.X);
        BinaryPrimitives.WriteSingleBigEndian(buffer[4..], value.Y);
        BinaryPrimitives.WriteSingleBigEndian(buffer[8..], value.Z);
        WriteBytes(buffer);
    }

    public void WriteVector4BE(Vector4 value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector4>()];
        BinaryPrimitives.WriteSingleBigEndian(buffer, value.X);
        BinaryPrimitives.WriteSingleBigEndian(buffer[4..], value.Y);
        BinaryPrimitives.WriteSingleBigEndian(buffer[8..], value.Z);
        BinaryPrimitives.WriteSingleBigEndian(buffer[12..], value.W);
        WriteBytes(buffer);
    }

    public void WriteMatrix4x4BE(Matrix4x4 value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Matrix4x4>()];

        Span<byte> r1 = buffer.Slice(0, 16);
        BinaryPrimitives.WriteSingleBigEndian(r1, value.M11);
        BinaryPrimitives.WriteSingleBigEndian(r1[4..], value.M12);
        BinaryPrimitives.WriteSingleBigEndian(r1[8..], value.M13);
        BinaryPrimitives.WriteSingleBigEndian(r1[12..], value.M14);

        Span<byte> r2 = buffer.Slice(16, 16);
        BinaryPrimitives.WriteSingleBigEndian(r2, value.M21);
        BinaryPrimitives.WriteSingleBigEndian(r2[4..], value.M22);
        BinaryPrimitives.WriteSingleBigEndian(r2[8..], value.M23);
        BinaryPrimitives.WriteSingleBigEndian(r2[12..], value.M24);

        Span<byte> r3 = buffer.Slice(32, 16);
        BinaryPrimitives.WriteSingleBigEndian(r3, value.M31);
        BinaryPrimitives.WriteSingleBigEndian(r3[4..], value.M32);
        BinaryPrimitives.WriteSingleBigEndian(r3[8..], value.M33);
        BinaryPrimitives.WriteSingleBigEndian(r3[12..], value.M34);

        Span<byte> r4 = buffer.Slice(48, 16);
        BinaryPrimitives.WriteSingleBigEndian(r4, value.M41);
        BinaryPrimitives.WriteSingleBigEndian(r4[4..], value.M42);
        BinaryPrimitives.WriteSingleBigEndian(r4[8..], value.M43);
        BinaryPrimitives.WriteSingleBigEndian(r4[12..], value.M44);

        WriteBytes(buffer);
    }

    public void WriteQuaternionBE(Quaternion value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Quaternion>()];
        BinaryPrimitives.WriteSingleBigEndian(buffer, value.X);
        BinaryPrimitives.WriteSingleBigEndian(buffer[4..], value.Y);
        BinaryPrimitives.WriteSingleBigEndian(buffer[8..], value.Z);
        BinaryPrimitives.WriteSingleBigEndian(buffer[12..], value.W);
        WriteBytes(buffer);
    }
}