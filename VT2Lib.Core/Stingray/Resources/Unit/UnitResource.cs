using VT2Lib.Core.Stingray.Scene;

namespace VT2Lib.Core.Stingray.Resources.Unit;

public abstract class UnitResource : Resource<UnitResource>
{
    public static IDString64 ID { get; } = "unit";

    public override IDString64 ResourceID => ID;

    public required MeshGeometry[] MeshGeometries { get; set; }

    public required SkinData[] SkinDatas { get; set; }

    public required byte[] SimpleAnimation { get; set; }

    public required SimpleAnimationGroup[] SimpleAnimationGroups { get; set; }

    public required SceneGraph SceneGraph { get; set; }

    public required MeshObject[] Meshes { get; set; }
}