namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal class BigConvexRawData
    {
        public ushort mSubdiv;
        public ushort mNbSamples;
        public byte[] mSamples;

        public uint mNbVerts;
        public uint mNbAdjVerts;
        public Valency mValencies;
        public byte[] mAdjacentVerts;
    }
}