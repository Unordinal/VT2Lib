namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    public static bool ReadHeader(Stream stream, ReadOnlySpan<byte> expectedHeader, out uint version, out bool mismatch)
    {
        version = 0;
        mismatch = false;

        Span<byte> buf = stackalloc byte[4];

        PhysX.ReadChunk(stream, buf);
        if (!buf[..3].SequenceEqual("ICE"u8))
            return false;

        int littleEndian = buf[3] & 1;
        mismatch = littleEndian != 1;

        PhysX.ReadChunk(stream, buf);
        if (!buf.SequenceEqual(expectedHeader))
            return false;

        version = PhysX.ReadDWord(stream, mismatch);
        return true;
    }

    public static void ReadIndices(Stream stream, ushort maxIndex, uint nbIndices, Span<ushort> indices, bool platformMismatch)
    {
        if (maxIndex <= 0xFF)
        {
            uint toSkip = nbIndices;
            stream.Position += toSkip;
        }
        else
        {
            stream.Position += nbIndices;
            //PhysX.ReadWordBuffer(stream, )
        }
    }

    public static bool WriteHeader(Stream stream, ReadOnlySpan<byte> header, uint version, bool mismatch)
    {
        int streamFlags = BitConverter.IsLittleEndian ? 1 : 0;
        if (mismatch)
            streamFlags ^= 1;

        Span<byte> buf = stackalloc byte[4];
        ("ICE"u8).CopyTo(buf);
        buf[3] = (byte)streamFlags;

        PhysX.WriteChunk(stream, buf);
        PhysX.WriteChunk(stream, header);
        PhysX.WriteDWord(stream, version, mismatch);
        return true;
    }
}