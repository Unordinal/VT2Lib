using System.Numerics;

namespace VT2Lib.Core.Stingray.Resources.Actor;

public class Shape
{
    public ShapeType Type { get; set; }

    public IDString32 Material { get; set; }

    public IDString32 ShapeTemplate { get; set; }

    public Matrix4x4 LocalTransform { get; set; }

    public byte[] ShapeData { get; set; } = null!; // PhysX data?

    public IDString32 ShapeNode { get; set; }

    //public Shape ShapeType { get; set; }
}

public class SphereShape : Shape
{
    public float Radius { get; set; }
}

public class BoxShape : Shape
{
    public Vector3 Radius { get; set; }
}

public class CapsuleShape : Shape
{
    public float Radius { get; set; }

    public float Height { get; set; }
}

public class MeshShape : Shape
{
    public IntPtr Mesh { get; set; }
}

public class ConvexShape : Shape
{
    public IntPtr Convex { get; set; }
}

public class HeightFieldShape : Shape
{
    public IntPtr HeightField { get; set; }

    public uint Columns { get; set; }

    public uint Rows { get; set; }

    public float HeightScale { get; set; }

    public float RowScale { get; set; }

    public float ColumnScale { get; set; }

    public byte MultipleMaterials { get; set; }
}