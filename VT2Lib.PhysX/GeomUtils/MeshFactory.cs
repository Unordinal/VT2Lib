using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal class MeshFactory
    {
        public static TriangleMeshData? LoadMeshData(Stream stream)
        {
            if (!PhysX.ReadHeader(stream, "MESH"u8, out uint version, out bool mismatch))
                return null;

            PxMeshMidPhase midphaseID = PxMeshMidPhase.eBVH33;
            if (version >= 14)
                midphaseID = (PxMeshMidPhase)PhysX.ReadDWord(stream, mismatch);

            if (version <= 9)
            {
                Trace.TraceError("Incompatible data; version <= 9");
                return null; // incompat
            }

            InternalMeshSerialFlag serialFlags = (InternalMeshSerialFlag)PhysX.ReadDWord(stream, mismatch);
            if (version <= 12)
                _ = PhysX.ReadFloat(stream, mismatch); // convexEdgeThreshold

            TriangleMeshData data = midphaseID switch
            {
                PxMeshMidPhase.eBVH33 => new RTreeTriangleData(),
                PxMeshMidPhase.eBVH34 => new BV4TriangleData(),
                _ => null
            };

            if (data is null)
            {
                Trace.TraceError($"Incompatible data; bad midphase ID {midphaseID}");
                return null;
            }

            uint nbVerts = PhysX.ReadDWord(stream, mismatch);
            data.mNbVertices = nbVerts;
            uint nbTris = PhysX.ReadDWord(stream, mismatch);
            data.mNbTriangles = nbTris;
            bool force32 = (serialFlags & (InternalMeshSerialFlag.IMSF_8BIT_INDICES | InternalMeshSerialFlag.IMSF_16BIT_INDICES)) == 0;

            stream.Position += Unsafe.SizeOf<Vector3>() * data.mNbVertices;

            uint nbIndices = 3 * data.mNbTriangles;
            if (serialFlags.HasFlag(InternalMeshSerialFlag.IMSF_8BIT_INDICES))
            {
                stream.Position += nbIndices;
            }
            else if (serialFlags.HasFlag(InternalMeshSerialFlag.IMSF_16BIT_INDICES))
            {
                stream.Position += nbIndices * sizeof(ushort);
            }
            else
            {
                stream.Position += nbIndices * sizeof(uint);
            }

            if (serialFlags.HasFlag(InternalMeshSerialFlag.IMSF_MATERIALS))
            {
                stream.Position += data.mNbTriangles * sizeof(ushort);
            }
            if (serialFlags.HasFlag(InternalMeshSerialFlag.IMSF_FACE_REMAP))
            {
                uint maxIndex = PhysX.ReadDWord(stream, mismatch);
                PhysX.ReadIndices(stream, maxIndex, data.mNbTriangles, null, mismatch);
            }
            if (serialFlags.HasFlag(InternalMeshSerialFlag.IMSF_ADJACENCIES))
            {
                stream.Position += data.mNbTriangles * 3 * sizeof(uint);
            }

            if (midphaseID == PxMeshMidPhase.eBVH33)
            {
                ((RTreeTriangleData)data).mRTree.Load(stream, version, mismatch);
            }
            else if (midphaseID == PxMeshMidPhase.eBVH34)
            {
                var bv4data = (BV4TriangleData)data;
                bv4data.mBV4Tree.Load(stream, mismatch);
                // set other bv4data stuff
            }

            data.mGeomEpsilon = PhysX.ReadFloat(stream, mismatch);
            stream.Position += 6 * sizeof(float);

            uint nb = PhysX.ReadDWord(stream, mismatch);
            if (nb > 0)
            {
                stream.Position += nb;
            }

            if (serialFlags.HasFlag(InternalMeshSerialFlag.IMSF_GRB_DATA))
            {
                uint GRB_meshAdjVerticesTotal = 0;
                if (version < 15)
                    GRB_meshAdjVerticesTotal = PhysX.ReadDWord(stream, mismatch);

                if (serialFlags.HasFlag(InternalMeshSerialFlag.IMSF_8BIT_INDICES))
                {
                    stream.Position += nbIndices;
                }
                else if (serialFlags.HasFlag(InternalMeshSerialFlag.IMSF_16BIT_INDICES))
                {
                    stream.Position += nbIndices * sizeof(ushort);
                }
                else
                {
                    stream.Position += nbIndices * sizeof(uint);
                }

                stream.Position += data.mNbTriangles * sizeof(uint) * 4;
                if (version < 15)
                {
                    stream.Position += data.mNbVertices * sizeof(uint);
                    stream.Position += data.mNbVertices * sizeof(uint);
                    stream.Position += GRB_meshAdjVerticesTotal * sizeof(uint);
                }
                stream.Position += data.mNbTriangles * sizeof(uint);

                var bv32Tree = new BV32Tree();
                data.mGRB_BV32Tree = bv32Tree;
                bv32Tree.Load(stream, mismatch);
            }

            return data;
        }
    }
}