using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.PhysX.Foundation;

namespace VT2Lib.PhysX.GeomUtils;

public static partial class Gu
{
    internal class ConvexHullData
    {
        public CenterExtents mAABB;
        public Vector3 mCenterOfMass;

        public PxBitAndDataT<ushort> mNbEdges;

        public byte mNbHullVertices;
        public byte mNbPolygons;

        public HullPolygonData mPolygons;
        public BigConvexRawData? mBigConvexRawData;

        public InternalObjectsData mInternal;
    }

}