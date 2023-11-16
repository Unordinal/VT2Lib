namespace VT2Lib.Core.Stingray.Scene;

public sealed class MeshGeometry
{
    public required VertexBuffer[] VertexBuffers { get; set; }

    public required IndexBuffer IndexBuffer { get; set; }

    public required BatchRange[] BatchRanges { get; set; }

    public required BoundingVolume BoundingVolume { get; set; }

    public required IDString32[] Materials { get; set; }
}