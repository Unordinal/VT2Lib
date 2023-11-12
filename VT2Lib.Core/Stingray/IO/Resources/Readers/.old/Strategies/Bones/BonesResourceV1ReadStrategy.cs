using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Resources;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers.Strategies.Bones;

internal class BonesResourceV1ReadStrategy : IResourceReadStrategy
{
    public BonesResourceV1 Read(Stream stream)
    {
        using var reader = new PrimitiveReader(stream, true);

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

        return new BonesResourceV1
        {
            BoneNameHashes = boneNameHashes,
            Lods = lods,
            BoneNames = boneNames
        };
    }

    IResource IResourceReadStrategy.Read(Stream stream)
    {
        throw new NotImplementedException();
    }
}