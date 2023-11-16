namespace VT2Lib.Core.Stingray.Resources.Bones;

public abstract class BonesResource : Resource<BonesResource>
{
    public static IDString64 ID { get; } = "bones";

    public override IDString64 ResourceID => ID;

    public required IDString32[] BoneNameHashes { get; set; }

    public required uint[] Lods { get; set; }

    public required string[] BoneNames { get; set; }

    public IEnumerable<IDString32> GetBoneNameIDStrings()
    {
        for (int i = 0; i < BoneNameHashes.Length; i++)
        {
            var boneNameIDString = BoneNameHashes[i];
            yield return boneNameIDString.Value ?? new IDString32(boneNameIDString.ID, BoneNames?.ElementAtOrDefault(i));
        }
    }
}