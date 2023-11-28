using Ai = Assimp;

namespace VT2Lib.Core.AssimpConversion.Exceptions;

internal class AiNodeMeshIndexOutOfRangeException : Exception
{
    public AiNodeMeshIndexOutOfRangeException(Ai.Node node, int meshIndex)
        : base($"Node '{node.Name}' has out of range mesh index '{meshIndex}'")
    {
    }
    
    public AiNodeMeshIndexOutOfRangeException(Ai.Node node, int meshIndex, int meshCount)
        : base($"Node '{node.Name}' has out of range mesh index '{meshIndex}' (total mesh count is {meshCount})")
    {
    }
}