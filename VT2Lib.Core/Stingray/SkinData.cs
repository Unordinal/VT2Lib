using System.Numerics;

namespace VT2Lib.Core.Stingray;

public sealed class SkinData
{
    public required Matrix4x4[] InvBindMatrices { get; set; }

    public required uint[] NodeIndices { get; set; }

    public required uint[][] MatrixIndexSets { get; set; }

    public IEnumerable<(uint NodeIndex, Matrix4x4 InvBindMatrix)> GetJointsBelongingToSet(uint indexSet)
    {
        if (indexSet >= MatrixIndexSets.Length)
            throw new ArgumentOutOfRangeException(nameof(indexSet));

        var setIndices = MatrixIndexSets[indexSet];
        var nodeIndices = setIndices.Select(idx => NodeIndices[idx]);
        var nodeIBMs = setIndices.Select(idx => InvBindMatrices[idx]);

        return setIndices.Zip(nodeIBMs);
    }
}