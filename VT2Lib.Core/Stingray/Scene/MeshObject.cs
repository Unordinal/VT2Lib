namespace VT2Lib.Core.Stingray.Scene;

public sealed class MeshObject
{
    public IDString32 Name { get; set; }

    public int NodeIndex { get; set; }

    public uint GeometryIndex { get; set; }

    public uint SkinIndex { get; set; }

    public RenderableFlags Flags { get; set; }

    public BoundingVolume BoundingVolume { get; set; }

    public bool HasGeometry()
    {
        return GeometryIndex > 0;
    }

    public bool HasSkin()
    {
        return SkinIndex > 0;
    }
}