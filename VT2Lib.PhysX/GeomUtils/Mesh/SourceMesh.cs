using System.Numerics;

namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal class SourceMeshBase
    {
        public uint mNbVerts;
        public Vector3[] mVerts;
        public uint[] mRemap;
    }

    internal class SourceMesh : SourceMeshBase
    {
        public uint mNbTris;
        public IndTri32[] mTriangles32;
        public IndTri16[] mTriangles16;
    }

    internal class IndTri32
    {
        public uint[] mRef;
    }

    internal class IndTri16
    {
        public ushort[] mRef;
    }
}