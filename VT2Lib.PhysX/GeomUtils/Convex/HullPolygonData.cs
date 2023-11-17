using VT2Lib.PhysX.Foundation;

namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal struct HullPolygonData
    {
        public PxPlane mPlane;
        public ushort mVRef8;
        public byte mNbVerts;
        public byte mMinIndex;
    }
}