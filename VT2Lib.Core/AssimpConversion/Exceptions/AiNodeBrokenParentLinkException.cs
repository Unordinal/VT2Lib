using Ai = Assimp;

namespace VT2Lib.Core.AssimpConversion.Exceptions;

internal class AiNodeBrokenParentLinkException : Exception
{
    public AiNodeBrokenParentLinkException(Ai.Node node)
        : base($"Parent of node '{node.Name}' does not contain it in its list of children.")
    {
    }
}