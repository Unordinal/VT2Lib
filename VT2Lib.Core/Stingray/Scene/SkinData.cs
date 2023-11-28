using System.Numerics;

namespace VT2Lib.Core.Stingray.Scene;

public sealed class SkinData
{
    public required Matrix4x4[] InvBindMatrices { get; set; }

    public required uint[] NodeIndices { get; set; }

    public required uint[][] MatrixIndexSets { get; set; }

    public IEnumerable<(uint NodeIndex, Matrix4x4 InvBindMatrix)> GetBonesForSet(uint boneSet)
    {
        if (boneSet >= MatrixIndexSets.Length)
            throw new ArgumentOutOfRangeException(nameof(boneSet));

        var setIndices = MatrixIndexSets[boneSet];

        var nodeIndices = setIndices.Select(idx => NodeIndices[idx]);
        var nodeIBMs = setIndices.Select(idx => InvBindMatrices[idx]);
        return nodeIndices.Zip(nodeIBMs);
    }
}