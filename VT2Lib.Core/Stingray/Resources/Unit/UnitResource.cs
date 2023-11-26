using System.Diagnostics.CodeAnalysis;
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

    public MeshGeometry GetObjectGeometry(MeshObject meshObject)
    {
        if (!meshObject.HasGeometry())
            throw new ArgumentOutOfRangeException(nameof(meshObject), "No geometry exists for the mesh object.");

        return MeshGeometries[meshObject.GeometryIndex - 1];
    }

    public bool TryGetObjectGeometry(MeshObject meshObject, [NotNullWhen(true)] out MeshGeometry? geometry)
    {
        bool hasGeometry = meshObject.HasGeometry();
        geometry = hasGeometry ? MeshGeometries[meshObject.GeometryIndex - 1] : null;
        return hasGeometry;
    }

    public SkinData GetObjectSkin(MeshObject meshObject)
    {
        if (!meshObject.HasSkin())
            throw new ArgumentOutOfRangeException(nameof(meshObject), "No skin data exists for the mesh object.");

        uint skinIndex = meshObject.SkinIndex - (uint)MeshGeometries.Length;
        return SkinDatas[skinIndex - 1];
    }

    public bool TryGetObjectSkin(MeshObject meshObject, [NotNullWhen(true)] out SkinData? skin)
    {
        bool hasSkin = meshObject.HasSkin();
        uint skinIndex = meshObject.SkinIndex - (uint)MeshGeometries.Length;
        skin = hasSkin ? SkinDatas[skinIndex - 1] : null;
        return hasSkin;
    }
}