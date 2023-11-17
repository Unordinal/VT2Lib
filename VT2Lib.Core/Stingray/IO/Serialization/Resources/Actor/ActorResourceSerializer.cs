using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Resources.Actor;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources.Actor;

public sealed class ActorResourceSerializer : ResourceSerializer<ActorResource>
{
    public static ActorResourceSerializer Default { get; } = new(SerializerFactory.Default.GetSerializer<IDString32>());

    private readonly ISerializer<IDString32> _idString32Serializer;
    private readonly ArraySerializer<Shape> _actorShapesSerializer;

    public ActorResourceSerializer(ISerializer<IDString32> idString32Serializer)
        : base(ActorResource.ID)
    {
        _idString32Serializer = idString32Serializer;
        _actorShapesSerializer = ArraySerializer.Create(new ActorResourceShapeSerializer(_idString32Serializer));
    }

    public override void Serialize(Stream stream, ActorResource resource)
    {
        throw new NotImplementedException();
    }

    public override ActorResource Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);
        return new ActorResource
        {
            Name = reader.ReadSerializable(_idString32Serializer),
            ActorTemplate = reader.ReadSerializable(_idString32Serializer),
            Node = reader.ReadSerializable(_idString32Serializer),
            Mass = reader.ReadSingleLE(),
            Shapes = reader.ReadSerializable(_actorShapesSerializer),
            OnStartTouch = reader.ReadUInt32LE(),
            OnStayTouching = reader.ReadUInt32LE(),
            OnEndTouch = reader.ReadUInt32LE(),
            OnTriggerEnter = reader.ReadUInt32LE(),
            OnTriggerLeave = reader.ReadUInt32LE(),
            OnTriggerStay = reader.ReadUInt32LE(),
            Enabled = reader.ReadByte()
        };
    }
}