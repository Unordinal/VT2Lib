using System.Runtime.CompilerServices;

namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal class BigConvexData
    {
        public BigConvexRawData mData;
        public byte[] mVBuffer;

        public bool Load(Stream stream)
        {
            if (!ReadHeader(stream, "SUPM"u8, out uint version, out bool mismatch))
                return false;

            if (!ReadHeader(stream, "GAUS"u8, out version, out mismatch))
                return false;

            mData = new BigConvexRawData();
            mData.mSubdiv = (ushort)PhysX.ReadDWord(stream, mismatch);
            mData.mNbSamples = (ushort)PhysX.ReadDWord(stream, mismatch);

            mData.mSamples = new byte[mData.mNbSamples * 2];
            stream.ReadExactly(mData.mSamples);

            return VLoad(stream);
        }

        public bool VLoad(Stream stream)
        {
            if (!ReadHeader(stream, "VALE"u8, out uint version, out bool mismatch))
                return false;

            mData.mNbVerts = PhysX.ReadDWord(stream, mismatch);
            mData.mNbAdjVerts = PhysX.ReadDWord(stream, mismatch);

            uint numVerts = (mData.mNbVerts + 3) & unchecked((uint)~3);
            uint totalSize = (uint)Unsafe.SizeOf<Valency>() * numVerts + mData.mNbAdjVerts;

            uint maxIndex = PhysX.ReadDWord(stream, mismatch);
            ReadIndices(stream, (ushort)maxIndex, mData.mNbVerts, null, mismatch);

            stream.Position += mData.mNbAdjVerts;

            //CreateOffsets();

            return true;
        }
    }
}