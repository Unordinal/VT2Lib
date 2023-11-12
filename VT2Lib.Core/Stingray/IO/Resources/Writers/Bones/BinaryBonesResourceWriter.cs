using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers.Bones;

public class BinaryBonesResourceWriter : ResourceWriter<BonesResource>
{
    public override void Write(Stream stream, BonesResource resource)
    {
        using var writer = new PrimitiveWriter(stream, true);

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