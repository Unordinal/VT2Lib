using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Core.AssimpConversion.Exceptions;
using VT2Lib.Core.Extensions;
using Ai = Assimp;

namespace VT2Lib.Core.AssimpConversion;

internal class AssimpSceneValidator
{
    private readonly Ai.Scene _aiScene;

    private AssimpSceneValidator(Ai.Scene aiScene)
    {
        ArgumentNullException.ThrowIfNull(aiScene);
        _aiScene = aiScene;
    }

    private void ValidateNodes()
    {
        foreach (var node in _aiScene.RootNode.TraverseChildren())
        {
            for (int i = 0; i < node.MeshIndices.Count; i++)
            {
                int meshIndex = node.MeshIndices[i];
                if (meshIndex >= _aiScene.MeshCount)
                    throw new AiNodeMeshIndexOutOfRangeException(node, meshIndex, _aiScene.MeshCount);

                //var aiMesh = _aiScene.Meshes[meshIndex]; // TODO: Validate bones?
            }

            if (!node.Parent.Children.Contains(node)) // Can happen because setting `node.Parent` does not reciprocate.
                throw new AiNodeBrokenParentLinkException(node);
        }
    }

    public static void ValidateScene(Ai.Scene aiScene)
    {
        var validator = new AssimpSceneValidator(aiScene);
        validator.ValidateNodes();
    }
}