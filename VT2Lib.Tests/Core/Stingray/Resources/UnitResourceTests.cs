using System.Diagnostics;
using System.Runtime.InteropServices;
using VT2Lib.Core.AssimpConversion;
using VT2Lib.Core.Native.PhysX;
using VT2Lib.Core.Numerics;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Hashing;
using VT2Lib.Core.Stingray.IO.Serialization.Resources;
using VT2Lib.Core.Stingray.Resources.Unit;
using VT2Lib.Core.Stingray.Scene;
using Xunit.Abstractions;

namespace VT2Lib.Tests.Core.Stingray.Resources;

public class UnitResourceTests
{
    private readonly ITestOutputHelper _output;

    public UnitResourceTests(ITestOutputHelper output)
    {
        _output = output;
        Trace.Listeners.Add(new TraceTestListener(_output));
    }

    [Fact]
    public void HashFiles()
    {
        string file = @"D:\Development\C#\Vermintide 2\VT2Lib\VT2Lib.Tests\TestFiles\Hash Dictionaries\vt2lib_hashdict64_types.txt";
        using var outFile = new StreamWriter(File.Create(@"D:\Development\C#\Vermintide 2\VT2Lib\VT2Lib.Tests\TestFiles\Hash Dictionaries\vt2lib_hashdict64_types_hashed.txt"));
        foreach (var line in File.ReadLines(file))
        {
            string output = $"{Murmur.Hash64(line):x16} {line}";
            outFile.WriteLine(output);
        }
    }

    [Theory]
    [MemberData(nameof(GetTestUnitResourceFiles))]
    public void ReadUnitFiles(string unitFile)
    {
        _output.WriteLine(Path.GetFileName(unitFile));
        using var unitStream = File.OpenRead(unitFile);
        var serializer = ResourceSerializerProvider.Default.GetSerializer(UnitResource.ID);

        var resource = serializer.DeserializeAs<UnitResource>(unitStream);
        _output.WriteLine(resource.ToString());
    }

    [Theory]
    [MemberData(nameof(GetManyUnitResourceFiles))]
    public void OutputUnitFilesSceneGraphs(string unitFile)
    {
        //_output.WriteLine(Murmur.Hash32("j_rightfingerbase").ToString("x8"));
        _output.WriteLine(Path.GetFileName(unitFile));

        HashDictUtil.PrepareKnownHashes();

        using var unitStream = File.OpenRead(unitFile);
        var serializer = ResourceSerializerProvider.Default.GetSerializer(UnitResource.ID);

        var unitResource = serializer.DeserializeAs<UnitResource>(unitStream);
        PrintAnyMatchingHashes(unitResource.SceneGraph);

        UnitAssimpSceneBuilder builder = UnitAssimpSceneBuilder.FromUnitResource(unitResource);
        PrintNodeTree(builder._aiScene.RootNode, 0);
        Assimp.AssimpContext ai = new();
        Directory.CreateDirectory(".conv_units");
        ai.ExportFile(builder.GetScene(), $".conv_units/{Path.GetFileName(unitFile)}.fbx", "fbx");

        void PrintNodeTree(Assimp.Node node, int depth)
        {
            _output.WriteLine($"{new string(' ', depth)}{node.Name} ({node.ChildCount})");
            foreach (var n in node.Children)
                PrintNodeTree(n, depth + 1);
        }

        PrintTree(unitResource, 0);
        /*foreach (var node in resource.SceneGraph.Nodes)
        {
            _output.WriteLine($"[{node.Name}]:");
            _output.WriteLine($"\tParent: {node.ParentIndex} ({node.ParentType})");
            _output.WriteLine($"\tLocal Transform: {node.LocalTransform}");
            _output.WriteLine($"\tWorld Transform: {node.WorldTransform}");
        }*/
    }

    private static IDString32[]? hashedMatching;

    private void PrintAnyMatchingHashes(SceneGraph nodes)
    {
        hashedMatching ??= File.ReadLines(HashDictUtil.HashSearchListFilePath).Select(l => new IDString32(l)).ToArray();
        foreach (var node in nodes.Nodes)
            if (node.Name.Value is null && hashedMatching.Contains(node.Name))
                _output.WriteLine($"{node.Name.ID:x8} {hashedMatching.First(id => id == node.Name).Value}");
    }

    private void PrintTree(UnitResource resource, int index, int indent = 0)
    {
        var nodes = resource.SceneGraph;
        var node = nodes.Nodes[index];

        var renderable = resource.Meshes.Where(m => m.NodeIndex == index).FirstOrDefault();
        var geo = renderable?.HasGeometry() == true ? resource.GetObjectGeometry(renderable) : null;
        var skin = renderable?.HasSkin() == true ? resource.GetObjectSkin(renderable) : null;

        var nodeSuffix = "";
        if (renderable != null)
        {
            var renderablePart = new MeshPartRenderable(resource, renderable);
            nodeSuffix += ' ' + renderablePart.ToString();
        }

        _output.WriteLine(new string('-', indent) + (indent > 0 ? " " : "") + $"[{index}]" + " " + node.Name.ToString() + nodeSuffix);
        if (geo != null)
        {
            var boneIndices = geo.VertexBuffers.Where(vb => vb.Channel.Component is VertexComponent.BlendIndices).FirstOrDefault();
            var boneWeights = geo.VertexBuffers.Where(vb => vb.Channel.Component is VertexComponent.BlendWeights).FirstOrDefault();
            if (boneIndices is not null)
            {
                Debug.Assert(boneWeights is not null);
                _output.WriteLine(boneIndices.ToString());
                _output.WriteLine(boneWeights.ToString());

                var vecIndices = MemoryMarshal.Cast<byte, Vector4A<byte>>(boneIndices.Data);
                var vecWeights = MemoryMarshal.Cast<byte, Vector4A<Half>>(boneWeights.Data);
                HashSet<byte> affectedBones = [];
                HashSet<byte> nonaffectedBones = [];
                for (int i = 0; i < vecIndices.Length; i++)
                {
                    var currIndices = vecIndices[i];
                    var currWeights = vecWeights[i];

                    if (currWeights.X != Half.Zero)
                        affectedBones.Add(currIndices.X);
                    else
                        nonaffectedBones.Add(currIndices.X);
                    if (currWeights.Y != Half.Zero)
                        affectedBones.Add(currIndices.Y);
                    else
                        nonaffectedBones.Add(currIndices.Y);
                    if (currWeights.Z != Half.Zero)
                        affectedBones.Add(currIndices.Z);
                    else
                        nonaffectedBones.Add(currIndices.Z);
                    if (currWeights.W != Half.Zero)
                        affectedBones.Add(currIndices.W);
                    else
                        nonaffectedBones.Add(currIndices.W);
                }

                _output.WriteLine("Affected bones: " + affectedBones.Count);
                _output.WriteLine("Nonaffected bones: " + nonaffectedBones.Count);
                _output.WriteLine("Nonaffected indices: " + string.Join(", ", nonaffectedBones));
            }
        }
        for (int i = 0; i < nodes.Nodes.Length; i++)
        {
            var curr = nodes.Nodes[i];
            if (curr.ParentType == ParentType.None || curr.ParentIndex != index)
                continue;

            PrintTree(resource, i, indent + 1);
        }
    }

    internal struct MeshPartRenderable
    {
        public IDString32 Name { get; }

        public MeshGeometry? Geometry { get; }

        public int GeoIndex { get; }

        public BatchRange[] BatchRanges { get; }

        public IDString32[] Materials { get; }

        public SkinData? Skin { get; }

        public int SkinIndex { get; }

        public MeshPartRenderable(UnitResource unit, MeshObject meshObject)
        {
            Name = meshObject.Name;
            if (meshObject.HasGeometry())
            {
                Geometry = unit.GetObjectGeometry(meshObject);
                BatchRanges = Geometry.BatchRanges;
                Materials = Geometry.Materials;
                GeoIndex = (int)meshObject.GeometryIndex - 1;
            }
            if (meshObject.HasSkin())
            {
                Skin = unit.GetObjectSkin(meshObject);
                SkinIndex = (int)meshObject.SkinIndex - unit.MeshGeometries.Length - 1;
            }
        }

        public override readonly string ToString()
        {
            string result = $"Renderable {Name}";
            /*if (Geometry is not null || Skin is not null)
                result += " ";*/

            if (Geometry is not null)
            {
                var materials = Materials;
                var batchRangesStr = BatchRanges.Select((br, i) => $"BR_{i}: <Range: ({br.Start}, {br.Size}); Mat: {materials[br.MaterialIndex]}; BoneSet: {br.BoneSet}>");
                result += $" (Geo[{GeoIndex}]: [{string.Join("; ", batchRangesStr)}])";
            }

            if (Skin is not null)
            {
                var skinStr = $" (Skin[{SkinIndex}]: {Skin.NodeIndices.Length} bones; {Skin.MatrixIndexSets.Length} MIS; {Skin.NodeIndices.Length} node indices; {Skin.InvBindMatrices.Length} IBM matrices)";
                for (int i = 0; i < Skin.MatrixIndexSets.Length; i++)
                {
                    uint[] mis = Skin.MatrixIndexSets[i];
                    skinStr += $"\nMIS[{i}]({mis.Length})";
                    skinStr += "[" + string.Join(", ", mis) + "]";
                }

                result += skinStr;
            }

            /*if (Geometry is not null || Skin is not null)
                result += ")";*/

            return '<' + result + '>';
        }
    }

    [Fact]
    public void Test()
    {
        using PhysXCommon_64 physx = new(@"G:\Games\Steam\steamapps\common\Warhammer Vermintide 2\binaries\PhysXCommon_64.dll");
        Trace.WriteLine(physx.PhysX_ReadDWord(null!));
    }

    public static IEnumerable<object[]> GetTestUnitResourceFiles()
    {
        yield return new object[] { @".extracted_resources_test\units\beings\critters\chr_critter_crow\chr_critter_crow.unit" };
        yield return new object[] { @".extracted_resources_test\units\architecture\broken_house\broken_house_floor_4m_01.unit" };
        yield return new object[] { @".extracted_resources_test\units\architecture\broken_house\broken_house_roof_4m_01.unit" }; // unit version 189?!
        yield return new object[] { @".extracted_resources_test\units\vegetation\town\town_veg_hedge_solid_low_01.unit" };
        yield return new object[] { @".extracted_resources_test\units\vegetation\town\town_veg_tree_dead_01.unit" };
    }

    public static IEnumerable<object[]> GetManyUnitResourceFiles()
    {
        //string basePath = @".extracted_resources_test\units\";
        string basePath = @".extracted_resources_test\units\beings\enemies\";
        foreach (var unitFile in Directory.EnumerateFiles(basePath, "*.unit", SearchOption.AllDirectories).Take(1000))
            yield return new object[] { unitFile };
    }

    public class TraceTestListener : TraceListener
    {
        private readonly ITestOutputHelper _output;

        public TraceTestListener(ITestOutputHelper output)
        {
            _output = output;
        }

        public override void Write(string? message)
        {
            _output.Write(message ?? string.Empty);
        }

        public override void WriteLine(string? message)
        {
            _output.WriteLine(message ?? string.Empty);
        }
    }
}