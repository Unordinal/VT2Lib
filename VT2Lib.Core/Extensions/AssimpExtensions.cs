using System.Numerics;
using System.Runtime.CompilerServices;
using VT2Lib.Core.Stingray;
using Ai = Assimp;

namespace VT2Lib.Core.Extensions;

internal static class AssimpExtensions
{
    public static Ai.Matrix4x4 ToAssimp(this Matrix4x4 value)
    {
        return Unsafe.BitCast<Matrix4x4, Ai.Matrix4x4>(value);
    }

    public static Ai.Vector3D ToAssimp(this Vector3 value)
    {
        return Unsafe.BitCast<Vector3, Ai.Vector3D>(value);
    }

    public static Ai.BoundingBox ToAssimpBoundingBox(this BoundingVolume boundingVolume)
    {
        Ai.Vector3D min = boundingVolume.LowerBounds.ToAssimp();
        Ai.Vector3D max = boundingVolume.UpperBounds.ToAssimp();
        return new Ai.BoundingBox(min, max);
    }

    public static IEnumerable<Ai.Node> TraverseChildren(this Ai.Node aiNode)
    {
        foreach (var child in aiNode.Children)
        {
            yield return child;
            foreach (var subchild in TraverseChildren(child))
                yield return subchild;
        }
    }
}