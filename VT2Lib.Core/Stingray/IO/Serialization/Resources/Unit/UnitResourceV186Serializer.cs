using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Resources.Actor;
using VT2Lib.Core.Stingray.Resources.Unit;
using VT2Lib.Core.Stingray.Scene;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources.Unit;

internal sealed class UnitResourceV186Serializer : ResourceSerializer<UnitResource>
{
    public static UnitResourceV186Serializer Default { get; } = new(SerializerFactory.Default, ResourceSerializerProvider.Default);

    private readonly ArraySerializer<MeshGeometry> _meshGeometriesSerializer;
    private readonly ArraySerializer<SkinData> _skinDatasSerializer;
    private readonly BufferSerializer<byte> _simpleAnimationSerializer;
    private readonly ArraySerializer<SimpleAnimationGroup> _simpleAnimationGroupsSerializer;
    private readonly ISerializer<SceneGraph> _sceneGraphSerializer;
    private readonly ArraySerializer<MeshObject> _meshObjectsSerializer;
    private readonly ArraySerializer<ActorResource> _actorResourcesSerializer;

    public UnitResourceV186Serializer(SerializerFactory serializerFactory, ResourceSerializerProvider resourceSerializerProvider)
        : base(UnitResource.ID)
    {
        _meshGeometriesSerializer = ArraySerializer.Create(serializerFactory.GetSerializer<MeshGeometry>());
        _skinDatasSerializer = ArraySerializer.Create(serializerFactory.GetSerializer<SkinData>());
        _simpleAnimationSerializer = BufferSerializer<byte>.Default;
        _simpleAnimationGroupsSerializer = ArraySerializer.Create(serializerFactory.GetSerializer<SimpleAnimationGroup>());
        _sceneGraphSerializer = serializerFactory.GetSerializer<SceneGraph>();
        _meshObjectsSerializer = ArraySerializer.Create(serializerFactory.GetSerializer<MeshObject>());
        _actorResourcesSerializer = ArraySerializer.Create(resourceSerializerProvider.GetSerializer<ActorResource>(ActorResource.ID));
    }

    public override void Serialize(Stream stream, UnitResource resource)
    {
        throw new NotImplementedException();
    }

    public override UnitResource Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);
        return new UnitResourceV186
        {
            MeshGeometries = reader.ReadSerializable(_meshGeometriesSerializer),
            SkinDatas = reader.ReadSerializable(_skinDatasSerializer),
            SimpleAnimation = reader.ReadSerializable(_simpleAnimationSerializer),
            SimpleAnimationGroups = reader.ReadSerializable(_simpleAnimationGroupsSerializer),
            SceneGraph = reader.ReadSerializable(_sceneGraphSerializer),
            Meshes = reader.ReadSerializable(_meshObjectsSerializer),
            Actors = null!, //reader.ReadSerializable(_actorResourcesSerializer), // uuugghhh, no thanks
        };
    }
}