using Ai = Assimp;
using System.Runtime.InteropServices;
using VT2Lib.Core.Numerics;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Resources.Unit;
using VT2Lib.Core.Stingray.Scene;

// typedefs
using AiColor4 = Assimp.Color4D;
using AiVector3 = Assimp.Vector3D;
using VT2Lib.Core.Extensions;

namespace VT2Lib.Core.AssimpConversion;

// TODO: Sort this class out, ugh - this could all be static rn probably
// check out stingray::UnitResourceBuilder, whewweee
internal sealed unsafe class UnitAssimpSceneBuilder : IDisposable
{
    public readonly Ai.Scene _aiScene;
    private readonly Dictionary<int, List<int>> _meshGeoIndexMap = [];
    private readonly Dictionary<uint, List<Ai.Material>> _meshGeoMaterialMap = [];
    private readonly Dictionary<uint, Ai.Bone[]> _meshBoneMap = [];

    public UnitAssimpSceneBuilder()
    {
        _aiScene = new Ai.Scene();
    }

    ~UnitAssimpSceneBuilder()
    {
        Dispose(disposing: false);
    }

    public Ai.Scene GetScene()
    {
        return _aiScene;
    }

    private void CreateMaterials(UnitResource unitResource)
    {
        /*foreach (var meshObj in unitResource.Meshes)
        {
            if (!meshObj.HasGeometry())
                continue;

            var materials = unitResource.GetObjectGeometry(meshObj).Materials;
            if (_meshGeoMaterialMap.ContainsKey(meshObj.GeometryIndex))
                throw new InvalidOperationException("Material map already contains geometry index");

            var aiMaterials = materials.Select(CreateDefaultMaterial).ToArray();
            _meshGeoMaterialMap[meshObj.GeometryIndex] = aiMaterials;
            _aiScene.Materials.AddRange(aiMaterials);
        }*/
    }

    private void CreateMeshes(UnitResource unitResource)
    {
        for (int i = 0; i < unitResource.Meshes.Length; i++)
        {
            MeshObject meshObj = unitResource.Meshes[i];
            if (!meshObj.HasGeometry())
                continue;

            var meshGeo = unitResource.GetObjectGeometry(meshObj);
            for (int j = 0; j < meshGeo.BatchRanges.Length; j++)
            {
                BatchRange batchRange = meshGeo.BatchRanges[j];
                var aiMesh = new Ai.Mesh(Ai.PrimitiveType.Triangle);
                SetMeshVertexBuffers(aiMesh, meshGeo.VertexBuffers);
                SetMeshIndexBuffer(aiMesh, meshGeo.IndexBuffer, batchRange);

                if (!_meshGeoIndexMap.ContainsKey(meshObj.NodeIndex))
                    _meshGeoIndexMap[meshObj.NodeIndex] = [];

                _aiScene.Meshes.Add(aiMesh);
                _meshGeoIndexMap[meshObj.NodeIndex].Add(_aiScene.MeshCount - 1);

                var material = meshGeo.Materials[batchRange.MaterialIndex];
                var aiMaterial = CreateDefaultMaterial(material);

                if (!_meshGeoMaterialMap.ContainsKey(meshObj.GeometryIndex))
                    _meshGeoMaterialMap[meshObj.GeometryIndex] = [];

                _meshGeoMaterialMap[meshObj.GeometryIndex].Add(aiMaterial);
                _aiScene.Materials.Add(aiMaterial);
                aiMesh.MaterialIndex = _aiScene.MaterialCount - 1;
            }
        }
    }

    private void AttachMeshes()
    {
        int index = 0;
        /*foreach (var node in TraverseChildren(_aiScene.RootNode))
        {
            if (_meshGeoIndexMap)
        }*/
    }

    private IEnumerable<Ai.Node> TraverseChildren(Ai.Node node)
    {
        foreach (var child in node.Children)
        {
            yield return child;
            foreach (var subchild in TraverseChildren(child))
                yield return subchild;
        }    
    }

    private void CreateBones(UnitResource unitResource)
    {
        foreach (var skin in unitResource.SkinDatas)
        {
        }
    }

    private void CreateNodeGraph(UnitResource unitResource)
    {
        // oh my god please kill all of this with fire it doesn't deserve to live
        _aiScene.RootNode = new Ai.Node("Assimp Root");
        var aiNodes = new List<Ai.Node>
        {
            _aiScene.RootNode
        };
        for (int i = 0; i < unitResource.SceneGraph.Nodes.Length; i++)
        {
            var sceneNode = unitResource.SceneGraph.Nodes[i];
            var aiNode = new Ai.Node(sceneNode.Name.ToString())
            {
                Transform = sceneNode.LocalTransform.ToAssimp()
            };
            if (_meshGeoIndexMap.TryGetValue(i, out var value))
                aiNode.MeshIndices.AddRange(value);
            aiNodes.Add(aiNode);
        }
        aiNodes[0].Children.Add(aiNodes[1]);
        for (int i = 0; i < aiNodes.Count - 1; i++)
        {
            var aiNode = aiNodes[i + 1];
            var sceneNode = unitResource.SceneGraph.Nodes[i];
            if (sceneNode.ParentType == ParentType.Internal)
            {
                aiNodes[sceneNode.ParentIndex + 1].Children.Add(aiNode);
            }
        }
    }

    private void CreateNodes(UnitResource unitResource, int graphIndex, Ai.Node aiLastNode)
    {
        if (graphIndex >= unitResource.SceneGraph.Nodes.Length)
            return;

        var sceneNode = unitResource.SceneGraph.Nodes[graphIndex];
        var aiNode = new Ai.Node(sceneNode.Name.ToString());
        /*if (sceneNode.ParentType == ParentType.Internal)
        {
            var parentSceneNode = unitResource.SceneGraph.Nodes[sceneNode.ParentIndex];
            var aiParentNode = _aiScene.RootNode.FindNode(parentSceneNode.Name.ToString());
            aiParentNode.Children.Add()
        }*/
        aiLastNode.Children.Add(aiNode);
    }

    private static void ClearMeshVertexBuffers(Ai.Mesh mesh)
    {
        mesh.Vertices.Clear();
        mesh.Normals.Clear();
        mesh.Tangents.Clear();
        mesh.BiTangents.Clear();
        foreach (var texcoords in mesh.TextureCoordinateChannels)
            texcoords.Clear();
        foreach (var colors in mesh.VertexColorChannels)
            colors.Clear();
    }

    private static void SetMeshVertexBuffers(Ai.Mesh aiMesh, VertexBuffer[] vertexBuffers)
    {
        // We'll do influencing bones (blendIndices/Weights) in another method. Feels kinda messy though...
        var vbsWithoutBoneChannels = vertexBuffers
            .Where(vb => vb.Channel.Component is not VertexComponent.BlendIndices and not VertexComponent.BlendWeights);

        ClearMeshVertexBuffers(aiMesh);
        foreach (var vertexBuffer in vbsWithoutBoneChannels)
        {
            switch (vertexBuffer.Channel.Component)
            {
                case VertexComponent.Position:
                    var positions = GetPositions(vertexBuffer);
                    aiMesh.Vertices.AddRange(positions);
                    break;

                case VertexComponent.Normal:
                    var normals = GetNormals(vertexBuffer);
                    aiMesh.Normals.AddRange(normals);
                    break;

                case VertexComponent.Tangent:
                    var tangents = GetTangents(vertexBuffer);
                    aiMesh.Tangents.AddRange(tangents);
                    break;

                case VertexComponent.Binormal:
                    var bitangents = GetBitangents(vertexBuffer);
                    aiMesh.BiTangents.AddRange(bitangents);
                    break;

                case VertexComponent.Texcoord:
                    var uvs = GetTexcoords(vertexBuffer);
                    var set = vertexBuffer.Channel.Set;
                    aiMesh.TextureCoordinateChannels[set].AddRange(uvs);
                    aiMesh.UVComponentCount[set] = 2;
                    break;

                case VertexComponent.Color:
                    var colors = GetColors(vertexBuffer);
                    aiMesh.VertexColorChannels[vertexBuffer.Channel.Set].AddRange(colors);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported vertex channel component type '{vertexBuffer.Channel.Component}'");
            }
        }
    }

    private static void SetMeshIndexBuffer(Ai.Mesh aiMesh, IndexBuffer indexBuffer, BatchRange batchRange)
    {
        const int IndicesPerFace = 3;
        var indicesBuffer = indexBuffer.EnumerateIndices().ToArray();
        bool success = aiMesh.SetIndices(indicesBuffer, IndicesPerFace);
        DebugEx.Assert(success);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            // TODO: dispose managed state (managed objects)
        }
    }

    private static Ai.Material CreateDefaultMaterial(IDString32 materialName)
    {
        return new Ai.Material
        {
            Name = materialName.ToString(),
            ColorDiffuse = new AiColor4(0.8f, 0.8f, 0.8f),
            Shininess = 0.5f,
            ShadingMode = Ai.ShadingMode.Fresnel
        };
    }

    public static UnitAssimpSceneBuilder FromUnitResource(UnitResource unitResource)
    {
        var builder = new UnitAssimpSceneBuilder();
        builder.CreateMeshes(unitResource);
        builder.CreateNodeGraph(unitResource);

        return builder;
    }

    #region VertexBuffer Conversion

    private static AiVector3[] GetPositions(VertexBuffer positions)
    {
        DebugEx.Assert(positions.Channel.Component == VertexComponent.Position);
        return GetAsVector3s(positions);
    }

    private static AiVector3[] GetNormals(VertexBuffer normals)
    {
        DebugEx.Assert(normals.Channel.Component == VertexComponent.Normal);
        return GetAsVector3s(normals);
    }

    private static AiVector3[] GetTangents(VertexBuffer tangents)
    {
        DebugEx.Assert(tangents.Channel.Component == VertexComponent.Tangent);
        return GetAsVector3s(tangents);
    }

    private static AiVector3[] GetBitangents(VertexBuffer bitangents)
    {
        DebugEx.Assert(bitangents.Channel.Component == VertexComponent.Binormal);
        return GetAsVector3s(bitangents);
    }

    private static AiVector3[] GetTexcoords(VertexBuffer texcoords)
    {
        DebugEx.Assert(texcoords.Channel.Component == VertexComponent.Texcoord);
        return GetAsVector3s(texcoords);
    }

    private static AiColor4[] GetColors(VertexBuffer colors)
    {
        DebugEx.Assert(colors.Channel.Component == VertexComponent.Color);
        ReadOnlySpan<byte> rawDataBytes = colors.Data;
        AiColor4[] aiData = new AiColor4[colors.Count];

        switch (colors.Channel.Type)
        {
            case ChannelType.UByte4_NORM:
            {
                var byteUnormVecs = MemoryMarshal.Cast<byte, Vector4A<byte>>(rawDataBytes);
                UnsafeEx.BufferConvert(byteUnormVecs, aiData, Vec4AByteUnormToColor4);
                break;
            }
            case ChannelType.Half4:
            {
                var halfVecs = MemoryMarshal.Cast<byte, Vector4A<Half>>(rawDataBytes);
                UnsafeEx.BufferConvert(halfVecs, aiData, Vec4AHalfToColor4);
                break;
            }
            case ChannelType.Float3:
            {
                var floatVecs = MemoryMarshal.Cast<byte, Vector3A<float>>(rawDataBytes);
                UnsafeEx.BufferConvert(floatVecs, aiData, Vec3AFloatToColor4);
                break;
            }
            default:
                throw new InvalidOperationException($"Unsupported channel type '{colors.Channel.Type}'");
        }

        return aiData;

        static AiColor4 Vec4AByteUnormToColor4(ref readonly Vector4A<byte> source)
        {
            float r = NormConversions.UnormToFloat(source.X);
            float g = NormConversions.UnormToFloat(source.Y);
            float b = NormConversions.UnormToFloat(source.Z);
            float a = NormConversions.UnormToFloat(source.W);

            return new AiColor4(r, g, b, a);
        }
    }

    private static Dictionary<int, List<Ai.VertexWeight>> GetWeights(VertexBuffer blendIndices, VertexBuffer blendWeights)
    {
        DebugEx.Assert(blendIndices.Channel.Type == ChannelType.UInt1); // treated like ChannelType.UByte4
        DebugEx.Assert(blendWeights.Channel.Type == ChannelType.Half4);

        // each byte is an index of a bone that corresponds with the float in the weights, signifying which bone affects the vert and by how much
        var vecIndices = new ReadOnlySpan<byte>(blendIndices.Data); //MemoryMarshal.Cast<byte, Vector4A<byte>>(blendIndices.Data);
        var vecWeights = MemoryMarshal.Cast<byte, Half>(new ReadOnlySpan<byte>(blendWeights.Data));

        var boneWeightMap = new Dictionary<int, List<Ai.VertexWeight>>();
        for (int i = 0; i < blendIndices.Count; i++)
        {
            int boneIndex = vecIndices[i];
            Half boneWeight = vecWeights[i];
            if (boneWeight == Half.Zero) // If zero, the weight doesn't matter and boneIndex will equal the index of the armature node.
                continue;

            var vertexWeight = new Ai.VertexWeight(i, (float)boneWeight);
            if (!boneWeightMap.ContainsKey(boneIndex))
                boneWeightMap[boneIndex] = [];

            boneWeightMap[boneIndex].Add(vertexWeight);
        }

        return boneWeightMap;
    }

    private static AiVector3[] GetAsVector3s(VertexBuffer vertexBuffer)
    {
        ReadOnlySpan<byte> rawData = vertexBuffer.Data;
        AiVector3[] aiData = new AiVector3[vertexBuffer.Count];
        switch (vertexBuffer.Channel.Type)
        {
            case ChannelType.Half2:
            {
                var halfVecs = MemoryMarshal.Cast<byte, Vector2A<Half>>(rawData);
                UnsafeEx.BufferConvert(halfVecs, aiData, Vec2AHalfToVec3);
                break;
            }
            case ChannelType.Half4:
            {
                var halfVecs = MemoryMarshal.Cast<byte, Vector4A<Half>>(rawData);
                UnsafeEx.BufferConvert(halfVecs, aiData, Vec4AHalfToVec3);
                break;
            }
            default:
                throw new InvalidOperationException($"Unsupported channel type '{vertexBuffer.Channel.Type}'");
        }

        return aiData;
    }

    private static void ConvertBufferStride(ReadOnlySpan<byte> source, Span<byte> destination, int inStride, int outStride)
    {
        DebugEx.Assert((source.Length / inStride) == (destination.Length / outStride));
        for (int srcIndex = 0, dstIndex = 0; srcIndex < source.Length && dstIndex < destination.Length; srcIndex += inStride, dstIndex += outStride)
        {
            source.Slice(srcIndex, inStride).CopyTo(destination[dstIndex..]);
        }
    }

    private static AiVector3 Vec2AHalfToVec3(ref readonly Vector2A<Half> source)
    {
        return new AiVector3((float)source.X, (float)source.Y, 0.0f);
    }

    private static AiVector3 Vec4AHalfToVec3(ref readonly Vector4A<Half> source)
    {
        return new AiVector3((float)source.X, (float)source.Y, (float)source.Z);
    }

    private static AiColor4 Vec4AHalfToColor4(ref readonly Vector4A<Half> source)
    {
        return new AiColor4((float)source.X, (float)source.Y, (float)source.Z, (float)source.W);
    }

    private static AiColor4 Vec3AFloatToColor4(ref readonly Vector3A<float> source)
    {
        return new AiColor4(source.X, source.Y, source.Z, 1.0f);
    }

    #endregion VertexBuffer Conversion

    internal record struct RenderableAssimpMap
    {
        public MeshGeometry[] MeshGeometries { get; }

        public MeshObject[] MeshObjects { get; }

        public SkinData[] SkinDatas { get; }
    }
}