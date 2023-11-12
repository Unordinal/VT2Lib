using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Resources;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers.Bones;

public class BinaryBonesResourceReader : ResourceReader<BonesResource>
{
    public override BonesResource Read(Stream stream)
    {
        PrimitiveReader reader = new(stream);

        uint numBones = reader.ReadUInt32LE();
        uint numLodLevels = reader.ReadUInt32LE();
        uint[] boneNameHashes = new uint[numBones];
        uint[] lods = new uint[numLodLevels];
        string[] boneNames = new string[numBones];

        for (int i = 0; i < numBones; i++)
            boneNameHashes[i] = reader.ReadUInt32LE();
        for (int i = 0; i < numLodLevels; i++)
            lods[i] = reader.ReadUInt32LE();
        for (int i = 0; i < numBones; i++)
            boneNames[i] = reader.ReadNullTermString();

        return new BonesResourceV0
        {
            BoneNameHashes = boneNameHashes,
            Lods = lods,
            BoneNames = boneNames
        };
    }
}