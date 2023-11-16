using System.Numerics;

namespace VT2Lib.Core.Stingray.Scene;

public sealed class SceneGraphData
{
    public Matrix4x4 LocalTransform { get; set; }

    public Matrix4x4 WorldTransform { get; set; }

    public ParentType ParentType { get; set; }

    public ushort ParentIndex { get; set; }

    public IDString32 Name { get; set; }
}