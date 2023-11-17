namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal class RTree
    {
        public const int RTREE_N = 4;

        public uint mPageSize;
        public uint mNumRootPages;
        public uint mNumLevels;
        public uint mTotalNodes;
        public uint mTotalPages;

        public bool Load(Stream stream, uint meshVersion, bool mismatch)
        {
            Span<byte> buf = stackalloc byte[4];
            PhysX.ReadChunk(stream, buf);
            if (!buf.SequenceEqual("RTRE"u8))
                return false;

            if (!PhysX.ReadBigEndianVersionNumber(stream, mismatch, out uint fileVersion, out mismatch))
                return false;

            stream.Position += 4 * sizeof(float) * 4;

            mPageSize = PhysX.ReadDWord(stream, mismatch);
            mNumRootPages = PhysX.ReadDWord(stream, mismatch);
            mNumLevels = PhysX.ReadDWord(stream, mismatch);
            mTotalNodes = PhysX.ReadDWord(stream, mismatch);
            mTotalPages = PhysX.ReadDWord(stream, mismatch);

            uint unused = PhysX.ReadDWord(stream, mismatch);

            for (int i = 0; i < mTotalPages; i++)
            {
                stream.Position += 4 * RTREE_N * 7;
            }

            return true;
        }
    }
}