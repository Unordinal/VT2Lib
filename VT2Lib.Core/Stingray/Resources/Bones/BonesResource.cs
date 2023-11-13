namespace VT2Lib.Core.Stingray.Resources.Bones;

public abstract class BonesResource : Resource<BonesResource>
{
    public static IDString64 ResourceID { get; } = "bones";

    public required uint[] BoneNameHashes { get; set; }

    public required uint[] Lods { get; set; }

    public required string[] BoneNames { get; set; }

    public IEnumerable<IDString32> GetBoneNameIDStrings()
    {
        for (int i = 0; i < BoneNameHashes.Length; i++)
            yield return new IDString32(BoneNameHashes[i], BoneNames?.ElementAtOrDefault(i));
    }
}