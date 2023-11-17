using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using VT2Lib.PhysX.Extensions;

namespace VT2Lib.PhysX.GeomUtils;

public static partial class PhysX
{
    public static void ReadChunk(Stream stream, Span<byte> destination)
    {
        stream.ReadExactly(destination[..4]);
    }

    public static bool ReadHeader(Stream stream, ReadOnlySpan<byte> expectedHeader, out uint version, out bool mismatch)
    {
        version = 0;
        mismatch = false;

        Span<byte> buf = stackalloc byte[4];

        ReadChunk(stream, buf);
        if (!buf[..3].SequenceEqual("NXS"u8))
        {
            Trace.TraceError($"Incompatible data; bad header (expected 'NXS', got '{Encoding.UTF8.GetString(buf)}' ({Convert.ToHexString(buf)}))");
            Trace.WriteLine("Position: " + stream.Position);
            return false;
        }

        Trace.WriteLine($"Read header 'NXS'");

        int littleEndian = buf[3] & 1;
        mismatch = littleEndian != 1;

        ReadChunk(stream, buf);
        if (!buf.SequenceEqual(expectedHeader))
        {
            Trace.TraceError($"Incompatible data; bad header (expected '{Encoding.UTF8.GetString(expectedHeader)}' ({Convert.ToHexString(expectedHeader)}), got '{Encoding.UTF8.GetString(buf)}' ({Convert.ToHexString(buf)}))");
            Trace.WriteLine("Position: " + stream.Position);
            return false;
        }

        version = ReadDWord(stream, mismatch);

        Trace.WriteLine($"Read header '{Encoding.UTF8.GetString(expectedHeader)}' ({Convert.ToHexString(expectedHeader)})");
        return true;
    }

    public static ushort ReadWord(Stream stream, bool mismatch)
    {
        ushort d = 0;
        stream.ReadStruct(ref d);
        if (mismatch)
            Flip(ref d);

        return d;
    }

    public static uint ReadDWord(Stream stream, bool mismatch)
    {
        uint d = 0;
        stream.ReadStruct(ref d);
        if (mismatch)
            Flip(ref d);

        return d;
    }

    public static float ReadFloat(Stream stream, bool mismatch)
    {
        float d = 0;
        stream.ReadStruct(ref d);
        if (mismatch)
            Flip(ref d);

        return d;
    }

    public static void WriteChunk(Stream stream, ReadOnlySpan<byte> chunk)
    {
        stream.Write(chunk);
    }

    public static bool WriteHeader(Stream stream, ReadOnlySpan<byte> header, uint version, bool mismatch)
    {
        int streamFlags = BitConverter.IsLittleEndian ? 1 : 0;
        if (mismatch)
            streamFlags ^= 1;

        Span<byte> buf = stackalloc byte[4];
        ("NXS?"u8).CopyTo(buf);
        buf[3] = (byte)streamFlags;

        WriteChunk(stream, buf);
        WriteChunk(stream, header);
        WriteDWord(stream, version, mismatch);
        return true;
    }

    public static void WriteWord(Stream stream, ushort value, bool mismatch)
    {
        if (mismatch)
            Flip(ref value);

        stream.Write(AsBytes(ref value));
    }

    public static void WriteDWord(Stream stream, uint value, bool mismatch)
    {
        if (mismatch)
            Flip(ref value);

        stream.Write(AsBytes(ref value));
    }

    public static bool ReadFloatBuffer(Stream stream, Span<float> destination, bool mismatch)
    {
        stream.ReadExactly(MemoryMarshal.AsBytes(destination));
        if (mismatch)
        {
            for (int i = 0; i < destination.Length; i++)
                Flip(ref destination[i]);
        }

        return true;
    }

    public static void ReadWordBuffer(Stream stream, Span<ushort> destination, bool mismatch)
    {
        stream.ReadExactly(MemoryMarshal.AsBytes(destination));
        if (mismatch)
        {
            for (int i = 0; i < destination.Length; i++)
                Flip(ref destination[i]);
        }
    }

    public static bool ReadDWordBuffer(Stream stream, Span<uint> destination, bool mismatch)
    {
        return ReadFloatBuffer(stream, MemoryMarshal.Cast<uint, float>(destination), mismatch);
    }

    public static void ReadIndices(Stream stream, uint maxIndex, uint nbIndices, Span<uint> destination, bool mismatch)
    {
        if (maxIndex <= 0xFF)
        {
            stream.Position += nbIndices;
        }
        else if (maxIndex <= 0xFFFF)
        {
            stream.Position += nbIndices * sizeof(ushort);
        }
        else
        {
            stream.Position += nbIndices * sizeof(uint);
        }
    }

    public static bool ReadBigEndianVersionNumber(Stream stream, bool mismatchIn, out uint fileVersion, out bool mismatch)
    {
        mismatch = BitConverter.IsLittleEndian;

        uint rawFileVersion = PhysX.ReadDWord(stream, false);
        if (rawFileVersion == 1)
        {
            fileVersion = 1;
        }
        else
        {
            uint fileVersionFlipped = rawFileVersion;
            PhysX.Flip(ref fileVersionFlipped);

            if (fileVersionFlipped == 1)
            {
                fileVersion = 1;
            }
            else
            {
                mismatch = mismatchIn;
                fileVersion = mismatchIn ? fileVersionFlipped : rawFileVersion;
            }
        }

        if (fileVersion > 3)
            return false;

        return true;
    }

    public static void Flip(ref ushort value)
    {
        Span<byte> buf = AsBytes(ref value);
        (buf[1], buf[0]) = (buf[0], buf[1]);
    }

    public static void Flip(ref short value)
    {
        Span<byte> buf = AsBytes(ref value);
        (buf[1], buf[0]) = (buf[0], buf[1]);
    }

    public static void Flip(ref uint value)
    {
        Span<byte> b = AsBytes(ref value);
        (b[3], b[2], b[1], b[0]) = (b[0], b[1], b[2], b[3]);
    }

    public static void Flip(ref int value)
    {
        Span<byte> b = AsBytes(ref value);
        (b[3], b[2], b[1], b[0]) = (b[0], b[1], b[2], b[3]);
    }

    public static void Flip(ref float value)
    {
        Span<byte> b = AsBytes(ref value);
        (b[3], b[2], b[1], b[0]) = (b[0], b[1], b[2], b[3]);
    }

    private static Span<byte> AsBytes<T>(ref T value)
        where T : unmanaged
    {
        return MemoryMarshal.AsBytes(new Span<T>(ref value));
    }
}