using System.Numerics;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Attributes;

namespace VT2Lib.Core.Stingray.Resources.Actor;

[StingrayResource]
public sealed class ActorResource : Resource<ActorResource>
{
    public const string ID = "actor";

    public override IDString64 ResourceID => ID;

    public IDString32 Name { get; set; }

    public IDString32 ActorTemplate { get; set; }

    public IDString32 Node { get; set; }

    public float Mass { get; set; }

    public Shape[] Shapes { get; set; } = null!;

    public uint OnStartTouch { get; set; }

    public uint OnStayTouching { get; set; }

    public uint OnEndTouch { get; set; }

    public uint OnTriggerEnter { get; set; }

    public uint OnTriggerLeave { get; set; }

    public uint OnTriggerStay { get; set; }

    public byte Enabled { get; set; }
}