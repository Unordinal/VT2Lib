using VT2Lib.Core.IO;

namespace VT2Lib.Core.Stingray.Resources.Readers;

public sealed class BonesResourceReader : ResourceReader<BonesResource>
{
    public override BonesResource Read(PrimitiveReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        BonesResource bones = new();

        bones.NumBones = reader.ReadUInt32LE();
        bones.NumLodLevels = reader.ReadUInt32LE();
        bones.BoneNameHashes = new uint[bones.NumBones];
        bones.Lods = new uint[bones.NumLodLevels];
        bones.BoneNames = new string[bones.NumBones];

        for (int i = 0; i < bones.NumBones; i++)
            bones.BoneNameHashes[i] = reader.ReadUInt32LE();
        for (int i = 0; i < bones.NumLodLevels; i++)
            bones.Lods[i] = reader.ReadUInt32LE();
        for (int i = 0; i < bones.NumBones; i++)
            bones.BoneNames[i] = reader.ReadNullTermString();

        return bones;
    }
}