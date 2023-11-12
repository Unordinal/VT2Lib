using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace VT2Lib.Core.IO;

public readonly ref partial struct PrimitiveWriter
{
    public void WriteUInt16LE(ushort value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteInt16LE(short value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteUInt32LE(uint value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteInt32LE(int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteUInt64LE(ulong value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteInt64LE(long value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteHalfLE(Half value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Half>()];
        BinaryPrimitives.WriteHalfLittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteSingleLE(float value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteDoubleLE(double value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteVector2LE(Vector2 value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector2>()];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value.X);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[4..], value.Y);
        WriteBytes(buffer);
    }

    public void WriteVector3LE(Vector3 value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector3>()];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value.X);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[4..], value.Y);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[8..], value.Z);
        WriteBytes(buffer);
    }

    public void WriteVector4LE(Vector4 value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Vector4>()];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value.X);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[4..], value.Y);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[8..], value.Z);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[12..], value.W);
        WriteBytes(buffer);
    }

    public void WriteMatrix4x4LE(Matrix4x4 value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Matrix4x4>()];

        Span<byte> r1 = buffer.Slice(0, 16);
        BinaryPrimitives.WriteSingleLittleEndian(r1, value.M11);
        BinaryPrimitives.WriteSingleLittleEndian(r1[4..], value.M12);
        BinaryPrimitives.WriteSingleLittleEndian(r1[8..], value.M13);
        BinaryPrimitives.WriteSingleLittleEndian(r1[12..], value.M14);

        Span<byte> r2 = buffer.Slice(16, 16);
        BinaryPrimitives.WriteSingleLittleEndian(r2, value.M21);
        BinaryPrimitives.WriteSingleLittleEndian(r2[4..], value.M22);
        BinaryPrimitives.WriteSingleLittleEndian(r2[8..], value.M23);
        BinaryPrimitives.WriteSingleLittleEndian(r2[12..], value.M24);

        Span<byte> r3 = buffer.Slice(32, 16);
        BinaryPrimitives.WriteSingleLittleEndian(r3, value.M31);
        BinaryPrimitives.WriteSingleLittleEndian(r3[4..], value.M32);
        BinaryPrimitives.WriteSingleLittleEndian(r3[8..], value.M33);
        BinaryPrimitives.WriteSingleLittleEndian(r3[12..], value.M34);

        Span<byte> r4 = buffer.Slice(48, 16);
        BinaryPrimitives.WriteSingleLittleEndian(r4, value.M41);
        BinaryPrimitives.WriteSingleLittleEndian(r4[4..], value.M42);
        BinaryPrimitives.WriteSingleLittleEndian(r4[8..], value.M43);
        BinaryPrimitives.WriteSingleLittleEndian(r4[12..], value.M44);

        WriteBytes(buffer);
    }

    public void WriteQuaternionLE(Quaternion value)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Quaternion>()];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value.X);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[4..], value.Y);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[8..], value.Z);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[12..], value.W);
        WriteBytes(buffer);
    }
}