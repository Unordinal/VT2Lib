namespace VT2Lib.Core.Stingray.Resources;

public sealed class BonesResource : IResource
{
    public static IDString64 ResourceType { get; } = "bones";

    public uint NumBones;

    public uint NumLodLevels;

    public uint[] BoneNameHashes = Array.Empty<uint>();

    public uint[] Lods = Array.Empty<uint>();

    public string[] BoneNames = Array.Empty<string>();
}