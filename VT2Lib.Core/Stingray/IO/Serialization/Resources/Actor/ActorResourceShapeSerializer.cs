using System.Diagnostics;
using System.Runtime.CompilerServices;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Resources.Actor;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources.Actor;

internal class ActorResourceShapeSerializer : ISerializer<Shape>
{
    private readonly ISerializer<IDString32> _idString32Serializer;

    public ActorResourceShapeSerializer(ISerializer<IDString32> idString32Serializer)
    {
        _idString32Serializer = idString32Serializer;
    }

    public void Serialize(Stream stream, Shape value)
    {
        throw new NotImplementedException();
    }

    // PhysX 3.X
    public Shape Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        ShapeType type = (ShapeType)reader.ReadUInt32LE();
        switch (type)
        {
            case ShapeType.Sphere:
            {
                Trace.WriteLine("Reading Actor SphereShape");
                SphereShape sphereShape = new() { Type = type };
                DeserializeCommon(in reader, sphereShape);
                sphereShape.Radius = reader.ReadSingleLE();
                Trace.WriteLine("Read Actor SphereShape " + stream.Position);
                return sphereShape;
            }
            case ShapeType.Box:
            {
                Trace.WriteLine("Reading Actor BoxShape");
                BoxShape boxShape = new() { Type = type };
                DeserializeCommon(in reader, boxShape);
                boxShape.Radius = reader.ReadVector3LE();
                Trace.WriteLine("Read Actor Shape " + stream.Position);
                return boxShape;
            }
            case ShapeType.Capsule:
            {
                Trace.WriteLine("Reading Actor CapsuleShape");
                CapsuleShape capsuleShape = new() { Type = type };
                DeserializeCommon(in reader, capsuleShape);
                capsuleShape.Radius = reader.ReadSingleLE();
                capsuleShape.Height = reader.ReadSingleLE();
                Trace.WriteLine("Read Actor Shape " + stream.Position);
                return capsuleShape;
            }
            case ShapeType.Mesh: // not implemented, skips.
            {
                Trace.WriteLine("Reading Actor MeshShape");
                MeshShape meshShape = new() { Type = type };
                DeserializeCommon(in reader, meshShape);
                var triangleMeshData = PhysX.GeomUtils.Gu.MeshFactory.LoadMeshData(stream); // mostly not implemented, skips
                if (triangleMeshData is null)
                {
                    Trace.TraceError("Failed loading TriangleMesh shape; serialization will fail.");
                    throw new InvalidDataException("Failed reading Actor TriangleMesh shape");
                }
                meshShape.Mesh = nint.Zero;
                stream.Position -= 4; // why?
                Trace.WriteLine("Read Actor Shape " + stream.Position);
                return meshShape;
            }
            case ShapeType.Convex: // not implemented, skips.
            {
                Trace.WriteLine("Reading Actor ConvexShape");
                ConvexShape convexShape = new() { Type = type };
                DeserializeCommon(in reader, convexShape);
                PhysX.GeomUtils.Gu.ConvexMesh guConvexMesh = new();
                bool guConvexSuccess = guConvexMesh.Load(reader.BaseStream);
                if (!guConvexSuccess)
                {
                    Trace.TraceError("Failed loading ConvexMesh shape; serialization will fail.");
                    throw new InvalidDataException("Failed reading Actor ConvexMesh shape");
                }

                stream.Position -= 4; // why?
                Trace.WriteLine("Read Actor Shape " + stream.Position);
                return convexShape;
            }
            case ShapeType.HeightField: // possibly broken/wrong
            {
                Trace.WriteLine("Reading Actor HeightFieldShape");
                HeightFieldShape heightFieldShape = new() { Type = type };
                DeserializeCommon(in reader, heightFieldShape);
                heightFieldShape.Columns = reader.ReadUInt32LE();
                heightFieldShape.Rows = reader.ReadUInt32LE();
                heightFieldShape.HeightScale = reader.ReadSingleLE();
                heightFieldShape.RowScale = reader.ReadSingleLE();
                heightFieldShape.ColumnScale = reader.ReadSingleLE();
                heightFieldShape.MultipleMaterials = reader.ReadByte();
                Trace.WriteLine("Read Actor Shape " + stream.Position);
                return heightFieldShape;
            }
            default:
                throw new InvalidDataException($"Encountered unimplemented ActorResource::Shape type: '{type}'");
        }
    }

    private void DeserializeCommon(in PrimitiveReader reader, Shape shape)
    {
        shape.Material = reader.ReadSerializable(_idString32Serializer);
        shape.ShapeTemplate = reader.ReadSerializable(_idString32Serializer);
        shape.LocalTransform = reader.ReadMatrix4x4LE();
        int shapeDataLength = reader.ReadInt32LE();
        shape.ShapeData = reader.ReadBytes(shapeDataLength);
        shape.ShapeNode = reader.ReadSerializable(_idString32Serializer);
    }
}