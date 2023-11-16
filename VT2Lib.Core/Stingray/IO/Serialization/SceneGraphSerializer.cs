using System.Numerics;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Scene;

namespace VT2Lib.Core.Stingray.IO.Serialization;

public sealed class SceneGraphSerializer : SerializerBase<SceneGraph>
{
    public static SceneGraphSerializer Default { get; } = new(SerializerFactory.Default.GetSerializer<IDString32>());

    private readonly ISerializer<IDString32> _idString32Serializer;

    public SceneGraphSerializer(ISerializer<IDString32> idString32Serializer)
    {
        ArgumentNullException.ThrowIfNull(idString32Serializer);
        _idString32Serializer = idString32Serializer;
    }

    public override void Serialize(Stream stream, SceneGraph value)
    {
        throw new NotImplementedException();
    }

    public override SceneGraph Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        int nodeCount = reader.ReadInt32LE();
        DebugEx.Assert(nodeCount >= 0);

        SceneGraphData[] nodes = new SceneGraphData[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            var node = nodes[i] = new SceneGraphData();

            Matrix4x4 rotation = reader.ReadMatrix3x3LE();
            Vector3 position = reader.ReadVector3LE();
            Vector3 scale = reader.ReadVector3LE();

            node.LocalTransform = MatrixUtil.GetTRSMatrix(position, rotation, scale);
        }

        foreach (var node in nodes)
        {
            node.WorldTransform = reader.ReadMatrix4x4LE();
        }

        foreach (var node in nodes)
        {
            node.ParentType = (ParentType)reader.ReadUInt16LE();
            node.ParentIndex = reader.ReadUInt16LE();
        }

        foreach (var node in nodes)
        {
            node.Name = reader.ReadSerializable(_idString32Serializer);
        }

        return new SceneGraph
        {
            Nodes = nodes,
        };
    }
}