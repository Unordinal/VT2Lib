using System.Numerics;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Scene;

namespace VT2Lib.Core.Stingray.IO.Serialization;

public sealed class SkinDataSerializer : SerializerBase<SkinData>
{
    public static SkinDataSerializer Default { get; } = new();

    public override void Serialize(Stream stream, SkinData value)
    {
        var writer = new PrimitiveWriter(stream);
        throw new NotImplementedException();
    }

    public override SkinData Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        int ibmCount = reader.ReadInt32LE();
        DebugEx.Assert(ibmCount >= 0);
        var ibms = new Matrix4x4[ibmCount];

        for (int i = 0; i < ibms.Length; i++)
            ibms[i] = reader.ReadMatrix4x4LE();

        int nodeIndicesCount = reader.ReadInt32LE();
        DebugEx.Assert(nodeIndicesCount >= 0);
        var nodeIndices = new uint[nodeIndicesCount];

        for (int i = 0; i < nodeIndices.Length; i++)
            nodeIndices[i] = reader.ReadUInt32LE();

        int matrixIndexSetCount = reader.ReadInt32LE();
        DebugEx.Assert(matrixIndexSetCount >= 0);
        var matrixIndexSets = new uint[matrixIndexSetCount][];

        for (int i = 0; i < matrixIndexSets.Length; i++)
        {
            int setIndicesCount = reader.ReadInt32LE();
            DebugEx.Assert(setIndicesCount >= 0);
            matrixIndexSets[i] = new uint[setIndicesCount];

            for (int j = 0; j < setIndicesCount; j++)
                matrixIndexSets[i][j] = reader.ReadUInt32LE();
        }

        return new SkinData
        {
            InvBindMatrices = ibms,
            NodeIndices = nodeIndices,
            MatrixIndexSets = matrixIndexSets,
        };
    }
}