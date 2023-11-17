using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using VT2Lib.PhysX.Foundation;

namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal class ConvexMesh
    {
        public ConvexHullData mHullData;
        public PxBitAndDataT<uint> mNb;

        public BigConvexData mBigConvexData;
        public float mMass;
        public Matrix4x4 mInteria; // PxMat33

        public IntPtr mMeshFactory; // GuMeshFactory*

        public bool Load(Stream stream)
        {
            if (!PhysX.ReadHeader(stream, "CVXM"u8, out uint version, out bool mismatch))
                return false;

            uint serialFlags = PhysX.ReadDWord(stream, mismatch);

            mHullData = new ConvexHullData();
            if (!ConvexHullLoad(stream, mHullData, out mNb))
                return false;

            Span<float> tmp = stackalloc float[8];
            PhysX.ReadFloatBuffer(stream, tmp, mismatch);
            // geomEpsilon = tmp[0];
            mHullData.mAABB = new CenterExtents()
            {
                mCenter = new Vector3(tmp[1], tmp[2], tmp[3]),
                mExtents = new Vector3(tmp[4], tmp[5], tmp[6])
            };

            mMass = tmp[7];
            if (mMass != -1.0f)
            {
                stream.Position += 9 * sizeof(float); // interia matr buf
                stream.Position += 3 * sizeof(float); // mHullData.mCenterOfMass
            }

            float gaussMapFlag = PhysX.ReadFloat(stream, mismatch);
            if (gaussMapFlag != -1.0f)
            {
                mBigConvexData = new BigConvexData();
                if (mBigConvexData != null)
                {
                    mBigConvexData.Load(stream);
                    mHullData.mBigConvexRawData = mBigConvexData.mData;
                }
            }

            stream.Position += 4 * sizeof(float);
            //PhysX.ReadFloatBuffer(stream, mHullData.mInternal.mRadius)
            return true;
        }

        private static bool ConvexHullLoad(Stream stream, ConvexHullData data, out PxBitAndDataT<uint> bufferSize)
        {
            bufferSize = default;
            if (!ReadHeader(stream, "CLHL"u8, out uint version, out bool mismatch))
                return false;

            if (version <= 8)
            {
                if (!ReadHeader(stream, "CVHL"u8, out version, out mismatch))
                    return false;
            }

            uint Nb;
            {
                Span<uint> tmp = stackalloc uint[4];
                PhysX.ReadDWordBuffer(stream, tmp, mismatch);
                data.mNbHullVertices = (byte)tmp[0];
                data.mNbEdges = (ushort)tmp[1];
                data.mNbPolygons = (byte)tmp[2];
                Nb = tmp[3];
            }

            uint bytesNeeded = ComputeBufferSize(data, Nb);
            bufferSize = Nb;

            int hullVertsLength = data.mNbHullVertices * 3;
            stream.Position += hullVertsLength;
            //PhysX.ReadFloatBuffer(stream, )

            if (version <= 6)
            {
                ushort useUnquantizedNormals = PhysX.ReadWord(stream, mismatch);
            }

            int polygonsLength = data.mNbPolygons * Unsafe.SizeOf<HullPolygonData>();
            stream.Position += polygonsLength;

            if (mismatch)
            {
                // flip polygons
            }

            stream.Position += Nb; // vertex data
            stream.Position += data.mNbEdges * 2; // facesByEdges
            if (version <= 5)
            {
                // compute face by verts
            }
            else
            {
                stream.Position += data.mNbHullVertices * 3;
            }

            if (data.mNbEdges.Bit)
            {
                if (version <= 7)
                {
                }
                else
                {
                    stream.Position += data.mNbEdges * 2;
                }
            }

            return true;
        }

        private static uint ComputeBufferSize(ConvexHullData data, uint nb)
        {
            int bytesNeeded = Unsafe.SizeOf<HullPolygonData>() * data.mNbPolygons;
            bytesNeeded += Unsafe.SizeOf<Vector3>() * data.mNbHullVertices;
            bytesNeeded += data.mNbEdges * 2;
            bytesNeeded += data.mNbHullVertices * 3;
            bytesNeeded += data.mNbEdges.Bit ? (sizeof(ushort) * data.mNbEdges * 2) : 0;
            bytesNeeded += (int)nb;

            int mod = bytesNeeded % sizeof(float);
            if (mod != 0)
                bytesNeeded += sizeof(float) - mod;

            return (uint)bytesNeeded;
        }
    }
}