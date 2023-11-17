using System.Numerics;
using System.Runtime.InteropServices;
using VT2Lib.PhysX.Foundation;

namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal enum InternalMeshSerialFlag : uint
    {
        IMSF_MATERIALS = (1 << 0),
        IMSF_FACE_REMAP = (1 << 1),
        IMSF_8BIT_INDICES = (1 << 2),
        IMSF_16BIT_INDICES = (1 << 3),
        IMSF_ADJACENCIES = (1 << 4),
        IMSF_GRB_DATA = (1 << 5),
    }

    internal enum PxMeshMidPhase : uint
    {
        eBVH33 = 0,
        eBVH34 = 1,

        eLast
    }

    internal class MeshDataBase
    {
        public PxMeshMidPhase mType;
        public byte mFlags;
        public uint mNbVertices;
        public Vector3[] mVertices;

        public PxBounds3 mAABB;
        public float mGeomEpsilon;

        public uint[] mFaceRemap;
        public uint[] mAdjacencies;

        public byte[] mGRB_primIndices;
        public byte[] mGRB_primAdjacencies;
        public uint[] mGRB_faceRemap;
    }

    internal class TriangleMeshData : MeshDataBase
    {
        public uint mNbTriangles;
        public byte[] mTriangles;

        public byte[] mExtraTrigData;
        public uint[] mMaterialIndices;

        public object mGRB_BV32Tree;
    }

    internal class RTreeTriangleData : TriangleMeshData
    {
        public RTree mRTree;

        public RTreeTriangleData()
        {
            mType = PxMeshMidPhase.eBVH33;
        }
    }

    internal class BV4TriangleData : TriangleMeshData
    {
        public SourceMesh mMeshInterface;
        public BV4Tree mBV4Tree;

        public BV4TriangleData()
        {
            mType = PxMeshMidPhase.eBVH34;
            mBV4Tree = new BV4Tree();
        }
    }

    internal class BV4Tree
    {
        public SourceMesh mMeshInterface;
        public LocalBounds mLocalBounds;

        public uint mNbNodes;
        public byte[] mNodes;
        public uint mInitData;
        public Vector3 mCenterOrMinCoeff;
        public Vector3 mExtentsOrMaxCoeff;
        public bool mUserAllocated;
        public bool mQuantized;
        public bool[] mPadding;

        public BV4Tree()
        {
            mMeshInterface = new();
        }

        public bool Load(Stream stream, bool mismatch)
        {
            Span<byte> buf = stackalloc byte[4];
            PhysX.ReadChunk(stream, buf);
            if (!buf.SequenceEqual("BV4 "u8))
                return false;

            if (!PhysX.ReadBigEndianVersionNumber(stream, mismatch, out uint fileVersion, out mismatch))
                return false;

            mLocalBounds.mCenter.X = PhysX.ReadFloat(stream, mismatch);
            mLocalBounds.mCenter.Y = PhysX.ReadFloat(stream, mismatch);
            mLocalBounds.mCenter.Z = PhysX.ReadFloat(stream, mismatch);
            mLocalBounds.mExtentsMagnitude = PhysX.ReadFloat(stream, mismatch);

            mInitData = PhysX.ReadDWord(stream, mismatch);

            stream.Position += 3 * sizeof(float) * 2;

            if (fileVersion >= 3)
            {
                uint quantized = PhysX.ReadDWord(stream, mismatch);
                mQuantized = quantized != 0;
            }
            else
            {
                mQuantized = true;
            }

            uint nbNodes = PhysX.ReadDWord(stream, mismatch);
            mNbNodes = nbNodes;

            if (nbNodes > 0)
            {
                uint nodeSize = 16; // sizeof(BVDataPackedQ) == sizeof(QuantizedAABB) + sizeof(uint)
                uint dataSize = nodeSize * nbNodes;

                stream.Position += dataSize;
            }
            else mNodes = null;

            return true;
        }
    }

    internal class BV32Tree
    {
        public SourceMesh mMeshInterface;
        public LocalBounds mLocalBounds;

        public uint mNbNodes;
        public BV32Data[] mNodes;
        public BV32DataPacked[] mPackedNodes;
        public uint mNbPackedNodes;
        public uint mInitData;
        public bool mUserAllocated;
        public bool[] mPadding; // 3

        public bool Load(Stream stream, bool mismatch)
        {
            Span<byte> buf = stackalloc byte[4];
            PhysX.ReadChunk(stream, buf);
            if (!buf.SequenceEqual("BV32"u8))
                return false;

            if (!PhysX.ReadBigEndianVersionNumber(stream, mismatch, out uint fileVersion, out mismatch))
                return false;

            mLocalBounds.mCenter.X = PhysX.ReadFloat(stream, mismatch);
            mLocalBounds.mCenter.Y = PhysX.ReadFloat(stream, mismatch);
            mLocalBounds.mCenter.Z = PhysX.ReadFloat(stream, mismatch);
            mLocalBounds.mExtentsMagnitude = PhysX.ReadFloat(stream, mismatch);

            mInitData = PhysX.ReadDWord(stream, mismatch);

            uint nbPackedNodes = PhysX.ReadDWord(stream, mismatch);
            mNbPackedNodes = nbPackedNodes;

            if (nbPackedNodes > 0)
            {
                mPackedNodes = new BV32DataPacked[nbPackedNodes];
                for (int i = 0; i < nbPackedNodes; i++)
                {
                    BV32DataPacked node = mPackedNodes[i] = new BV32DataPacked();
                    node.mNbNodes = PhysX.ReadDWord(stream, mismatch);
                    node.mData = new uint[node.mNbNodes];
                    PhysX.ReadDWordBuffer(stream, node.mData, mismatch);

                    uint nbElements = 4 * node.mNbNodes;
                    stream.Position += nbElements * sizeof(float) * 2;
                }
            }

            return true;
        }
    }

    internal class BV32Data
    {
        public Vector3 mCenter;
        public uint mNbLeafNodes;
        public Vector3 mExtents;
        public nuint mData;
    }

    internal class BV32DataPacked
    {
        public Vector4[] mCenter; // 32
        public Vector4[] mExtents; // 32
        public uint[] mData; // 32
        public uint mNbNodes;
        public uint[] pad; // 3
    }

    internal struct LocalBounds
    {
        public Vector3 mCenter;
        public float mExtentsMagnitude;
    }
}