﻿using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Attributes;

namespace VT2Lib.Core.Stingray.Resources.Bones;

[StingrayResource(StingrayResourceAttribute.Versionless)]
public sealed class BonesResourceV0 : BonesResource
{
    public static BonesResourceV0 ReadBinary(in PrimitiveReader reader)
    {
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

    public static void WriteBinary(Stream stream, BonesResourceV0 resource)
    {
        PrimitiveWriter writer = new(stream);

        writer.WriteInt32LE(resource.BoneNameHashes.Length);
        writer.WriteInt32LE(resource.Lods.Length);
        foreach (var nameHash in resource.BoneNameHashes)
            writer.WriteUInt32LE(nameHash);

        foreach (var lod in resource.Lods)
            writer.WriteUInt32LE(lod);

        foreach (var name in resource.BoneNames)
            writer.WriteNullTermString(name);
    }
}