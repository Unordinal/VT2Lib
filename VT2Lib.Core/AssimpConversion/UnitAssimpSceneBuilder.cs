using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.Numerics;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Resources.Unit;
using VT2Lib.Core.Stingray.Scene;
using Ai = Assimp;

// typedefs
using AiColor4 = Assimp.Color4D;
using AiVector3 = Assimp.Vector3D;

namespace VT2Lib.Core.AssimpConversion;

// TODO: Sort this class out, ugh - this could all be static rn probably
// check out stingray::UnitResourceBuilder, whewweee
internal sealed unsafe class UnitAssimpSceneBuilder : IDisposable
{
    public readonly Ai.Scene _aiScene;
    private readonly Dictionary<int, List<int>> _meshGeoIndexMap = [];
    private readonly Dictionary<uint, List<Ai.Material>> _meshGeoMaterialMap = [];
    private readonly Dictionary<uint, Ai.Bone[]> _meshBoneMap = [];

    private readonly Dictionary<IDString32, int> _sceneMaterialIndexMap = [];
    private readonly Dictionary<(MeshGeometry, BatchRange), int> _sceneMeshIndexMap = [];
    private readonly Dictionary<int, List<int>> _sceneNodeToMeshIndicesMap = [];
    private Ai.Node[] _sceneNodeFlatList;

    public UnitAssimpSceneBuilder(UnitResource unitResource)
    {
        _aiScene = new Ai.Scene();
        _aiScene.Metadata["UnitScaleFactor"] = new Ai.Metadata.Entry(Ai.MetaDataType.Float, 100.0f);
        _sceneNodeFlatList = CreateAndAddNodes(unitResource);
    }

    ~UnitAssimpSceneBuilder()
    {
        Dispose(disposing: false);
    }

    public Ai.Scene GetScene()
    {
        return _aiScene;
    }

    private void CreateAndAddToScene(UnitResource unitResource)
    {
        DebugEx.Assert(_sceneNodeFlatList is not null && _sceneNodeFlatList.Length > 0);

        for (int i = 0; i < unitResource.Meshes.Length; i++)
        {
            MeshObject meshObj = unitResource.Meshes[i];
            if (!meshObj.HasGeometry())
                continue;

            Ai.Node aiNode = _sceneNodeFlatList[meshObj.NodeIndex];
            MeshGeometry meshGeo = unitResource.GetObjectGeometry(meshObj);
            List<int> sceneMeshIndices = _sceneNodeToMeshIndicesMap[meshObj.NodeIndex] = [];

            for (int j = 0; j < meshGeo.BatchRanges.Length; j++)
            {
                BatchRange batchRange = meshGeo.BatchRanges[j];

                var (sceneMeshIndex, aiMesh) = GetOrAddAiMesh(meshGeo, batchRange);
                if (!sceneMeshIndices.Contains(sceneMeshIndex))
                    sceneMeshIndices.Add(sceneMeshIndex);

                aiNode.MeshIndices.Add(sceneMeshIndex);
            }

            if (meshObj.HasSkin())
                CreateAndAddAiBones(unitResource, meshObj);
        }

        //AddMeshesToNodes(unitResource, _sceneNodeFlatList);
    }

    private (int SceneMeshIndex, Ai.Mesh AiMesh) GetOrAddAiMesh(MeshGeometry meshGeometry, BatchRange batchRange)
    {
        var dictKey = (meshGeometry, batchRange);
        if (!_sceneMeshIndexMap.TryGetValue(dictKey, out int sceneMeshIndex))
        {
            var aiMesh = CreateAiMesh(meshGeometry, batchRange);
            _aiScene.Meshes.Add(aiMesh);
            sceneMeshIndex = _aiScene.MeshCount - 1;
            _sceneMeshIndexMap[dictKey] = sceneMeshIndex;
        }

        return (sceneMeshIndex, _aiScene.Meshes[sceneMeshIndex]);
    }

    private Ai.Mesh CreateAiMesh(MeshGeometry meshGeometry, BatchRange batchRange)
    {
        var aiMesh = new Ai.Mesh(Ai.PrimitiveType.Triangle);
        SetMeshVertexBuffers(aiMesh, meshGeometry.VertexBuffers);
        SetMeshIndexBuffer(aiMesh, meshGeometry.IndexBuffer, batchRange);
        aiMesh.BoundingBox = meshGeometry.BoundingVolume.ToAssimpBoundingBox();

        var material = meshGeometry.Materials[batchRange.MaterialIndex];
        aiMesh.MaterialIndex = GetOrAddAiMaterial(material).SceneMaterialIndex;
        return aiMesh;
    }

    private (int SceneMaterialIndex, Ai.Material AiMaterial) GetOrAddAiMaterial(IDString32 materialName)
    {
        if (!_sceneMaterialIndexMap.TryGetValue(materialName, out int sceneMaterialIndex))
        {
            var aiMaterial = CreateAiMaterial(materialName);
            _aiScene.Materials.Add(aiMaterial);
            _sceneMaterialIndexMap[materialName] = _aiScene.MaterialCount - 1;
        }

        return (sceneMaterialIndex, _aiScene.Materials[sceneMaterialIndex]);
    }

    private Ai.Material CreateAiMaterial(IDString32 materialName)
    {
        return new Ai.Material
        {
            Name = materialName.ToString(),
            ColorDiffuse = new AiColor4(0.8f, 0.8f, 0.8f),
            Shininess = 1.0f,
            ShininessStrength = 0.5f,
            ShadingMode = Ai.ShadingMode.Fresnel
        };
    }

    private void CreateAndAddAiBones(UnitResource unitResource, MeshObject meshObject)
    {
        DebugEx.Assert(_sceneNodeFlatList is not null);
        DebugEx.Assert(meshObject.HasGeometry() && meshObject.HasSkin());

        MeshGeometry meshGeo = unitResource.GetObjectGeometry(meshObject);
        VertexBuffer? vbBlendIndices = meshGeo.VertexBuffers.FirstOrDefault(vb => vb.Channel.Component is VertexComponent.BlendIndices);
        VertexBuffer? vbBlendWeights = meshGeo.VertexBuffers.FirstOrDefault(vb => vb.Channel.Component is VertexComponent.BlendWeights);

        if (vbBlendIndices is null || vbBlendWeights is null)
            throw new InvalidDataException($"Missing vertex weight data for object with assigned SkinData (Missing: {DebugEx.ListNulls(vbBlendIndices, vbBlendWeights)})");
        if (vbBlendIndices.Channel.Type != ChannelType.UInt1) // ChannelType.UByte4
            throw new InvalidDataException($"Indices are in unexpected format '{vbBlendIndices.Channel.Type}'; expected '{ChannelType.UInt1}'");
        if (vbBlendWeights.Channel.Type != ChannelType.Half4)
            throw new InvalidDataException($"Weights are in unexpected format '{vbBlendWeights.Channel.Type}'; expected '{ChannelType.Half4}'");

        DebugEx.Assert(vbBlendIndices.Count == vbBlendWeights.Count);
        DebugEx.Assert(vbBlendIndices.Stride * 2 == vbBlendWeights.Stride);
        DebugEx.Assert(vbBlendIndices.Data.Length * 2 == vbBlendWeights.Data.Length);

        ReadOnlySpan<Vector4A<byte>> blendIndices = MemoryMarshal.Cast<byte, Vector4A<byte>>(vbBlendIndices.Data);
        ReadOnlySpan<Vector4A<Half>> blendWeights = MemoryMarshal.Cast<byte, Vector4A<Half>>(vbBlendWeights.Data);
        DebugEx.Assert(blendIndices.Length == blendWeights.Length);

        IndexBuffer ibVertexIndices = meshGeo.IndexBuffer;
        ReadOnlySpan<int> vertexIndices = ibVertexIndices.EnumerateIndices().ToArray();
        SkinData skinData = unitResource.GetObjectSkin(meshObject);
#if DEBUG
        HashSet<uint> seenBoneSets = [];
#endif
        foreach (BatchRange batchRange in meshGeo.BatchRanges)
        {
#if DEBUG
            DebugEx.Assert(seenBoneSets.Add(batchRange.BoneSet));
#endif
            var batchVertexIndices = vertexIndices.Slice(batchRange.GetVertStartIndex(), batchRange.GetVertCount());

            var bones = skinData.GetBonesForSet(batchRange.BoneSet).ToArray();
            var aiBones = bones.Select(b =>
            {
                var node = unitResource.SceneGraph.Nodes[b.NodeIndex];
                var nodeName = node.Name.ToString();
                var ibm = b.InvBindMatrix.ToAssimp();
                var aiBone = new Ai.Bone(nodeName, ibm, null);

                var aiMeshIndex = _sceneMeshIndexMap[(meshGeo, batchRange)];
                var aiMesh = _aiScene.Meshes[aiMeshIndex];

                /*int idxOfMesh = unitResource.Meshes.IndexOf(m => m.NodeIndex == b.NodeIndex);
                if (idxOfMesh == -1)
                    throw new InvalidOperationException("No mesh found with node index of bone");
                var boneMeshObj = unitResource.Meshes[idxOfMesh];
                if (!boneMeshObj.HasGeometry() || !boneMeshObj.HasSkin())
                    throw new InvalidOperationException("Found bone mesh object missing geometry or skin");
                var boneMeshGeo = unitResource.GetObjectGeometry(boneMeshObj);*/

                aiMesh.Bones.Add(aiBone); // wrong? this is adding to _this_ mesh a new bone. we probably want to add to 'b.NodeIndex'... I think? oh I don't knowwwww, I did it this way last time apparently and it worked okay
                return aiBone;
            }).ToArray();

            for (int i = 0; i < batchVertexIndices.Length; i++)
            {
                int vertexIndex = batchVertexIndices[i];
                // 'bone' refers to an inverse bind matrix in this case; they're interchangeable.
                // Each index in a bone indices vector [x, y, z, w] is a byte index into the list of matrices
                // in the skin data.
                // Each vertex weight is a weight for that bone.
                // See https://learn.microsoft.com/en-us/windows/win32/direct3d9/indexed-vertex-blending
                var boneIndices = blendIndices[vertexIndex];
                var boneWeights = blendWeights[vertexIndex];

                for (int j = 0; j < Vector4A.Count; j++)
                {
                    var boneIndex = boneIndices[j];
                    var boneWeight = boneWeights[j];
                    if (boneWeight == Half.Zero)
                        continue;

                    var vertexWeight = new Ai.VertexWeight(vertexIndex, (float)boneWeight);
                    aiBones[boneIndex].VertexWeights.Add(vertexWeight);
                }
            } 
        }
    }

    private Ai.Node[] CreateAndAddNodes(UnitResource unitResource)
    {
        var sceneGraph = unitResource.SceneGraph;
        var aiNodeList = new Ai.Node[sceneGraph.Nodes.Length];

        _aiScene.RootNode = new Ai.Node("Assimp Root Node");

        for (int i = 0; i < aiNodeList.Length; i++)
        {
            var sceneNode = sceneGraph.Nodes[i];
            var sceneNodeName = sceneNode.Name.ToString();

            var parent = sceneNode.ParentType != ParentType.None
                ? aiNodeList[sceneNode.ParentIndex]
                : _aiScene.RootNode;

            var aiNode = new Ai.Node(sceneNodeName, parent);
            parent.Children.Add(aiNode);
            aiNode.Transform = Matrix4x4.Transpose(sceneNode.LocalTransform).ToAssimp();

            aiNodeList[i] = aiNode;
        }

        return aiNodeList;
    }

    private void AddMeshesToNodes(UnitResource unitResource, Ai.Node[] aiNodeList)
    {
        foreach (var mesh in unitResource.Meshes)
        {
            if (!mesh.HasGeometry())
                continue;

            var aiNode = aiNodeList[mesh.NodeIndex];
            aiNode.MeshIndices.AddRange(_sceneNodeToMeshIndicesMap[mesh.NodeIndex]);
        }
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
        var indicesBuffer = indexBuffer.EnumerateIndices(batchRange, IndicesPerFace).ToArray();
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

    public static UnitAssimpSceneBuilder FromUnitResource(UnitResource unitResource)
    {
        var builder = new UnitAssimpSceneBuilder(unitResource);
        builder.CreateAndAddToScene(unitResource);

        AssimpSceneValidator.ValidateScene(builder.GetScene());
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