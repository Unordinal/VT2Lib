using System.Numerics;
using VT2Lib.Core.Stingray.Resources.Readers;

namespace VT2Lib.Core.Stingray.Resources;

public sealed class BonesResource : IResource
{
    public static IDString64 ResourceType { get; } = "bones";

    public uint NumBones;

    public uint NumLodLevels;

    public uint[] BoneNameHashes = Array.Empty<uint>();

    public uint[] Lods = Array.Empty<uint>();

    public string[] BoneNames = Array.Empty<string>();

    public IEnumerable<IDString32> GetBoneNameIDStrings()
    {
        for (int i = 0; i < BoneNameHashes.Length; i++)
            yield return new IDString32(BoneNameHashes[i], BoneNames.ElementAtOrDefault(i));
    }
}