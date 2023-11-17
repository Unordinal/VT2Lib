using VT2Lib.Core.Extensions;
using VT2Lib.Core.IO;
using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources.Bones;

public sealed class BonesResourceSerializer : ResourceSerializer<BonesResource>
{
    public static BonesResourceSerializer Default { get; } = new(SerializerFactory.Default.GetSerializer<IDString32>());

    private readonly ISerializer<IDString32> _idString32Serializer;

    public BonesResourceSerializer(ISerializer<IDString32> idString32Serializer)
        : base(BonesResource.ID)
    {
        _idString32Serializer = idString32Serializer;
    }

    public override void Serialize(Stream stream, BonesResource bones)
    {
        var writer = new PrimitiveWriter(stream);

        writer.WriteInt32LE(bones.BoneNameHashes.Length);
        writer.WriteInt32LE(bones.Lods.Length);
        foreach (var nameHash in bones.BoneNameHashes)
            writer.WriteSerializable(nameHash, _idString32Serializer);

        foreach (var lod in bones.Lods)
            writer.WriteUInt32LE(lod);

        foreach (var name in bones.BoneNames)
            writer.WriteNullTermString(name);
    }

    // Should the bone name hashes be IDString32s?
    public override BonesResource Deserialize(Stream stream)
    {
        var reader = new PrimitiveReader(stream);

        int numBones = reader.ReadInt32LE();
        int numLodLevels = reader.ReadInt32LE();
        IDString32[] boneNameHashes = new IDString32[numBones];
        uint[] lods = new uint[numLodLevels];
        string[] boneNames = new string[numBones];

        for (int i = 0; i < boneNameHashes.Length; i++)
            boneNameHashes[i] = reader.ReadSerializable(_idString32Serializer);
        for (int i = 0; i < lods.Length; i++)
            lods[i] = reader.ReadUInt32LE();
        for (int i = 0; i < boneNames.Length; i++)
            boneNames[i] = reader.ReadNullTermString();

        return new BonesResourceV0
        {
            BoneNameHashes = boneNameHashes,
            Lods = lods,
            BoneNames = boneNames
        };
    }
}