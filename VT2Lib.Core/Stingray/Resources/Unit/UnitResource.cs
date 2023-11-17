using VT2Lib.Core.Stingray.Resources.Actor;
using VT2Lib.Core.Stingray.Scene;

namespace VT2Lib.Core.Stingray.Resources.Unit;

public abstract class UnitResource : Resource<UnitResource>
{
    public const string ID = "unit";

    public override IDString64 ResourceID => ID;

    public required MeshGeometry[] MeshGeometries { get; set; }

    public required SkinData[] SkinDatas { get; set; }

    public required byte[] SimpleAnimation { get; set; }

    public required SimpleAnimationGroup[] SimpleAnimationGroups { get; set; }

    public required SceneGraph SceneGraph { get; set; }

    public required MeshObject[] Meshes { get; set; }

    public required ActorResource[] Actors { get; set; }
}