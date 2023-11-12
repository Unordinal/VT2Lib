using System.Diagnostics;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers.Binary;

public sealed class BonesBinaryResourceWriter : BinaryResourceWriter<BonesResource>
{
    public override void Write(BinaryWriter writer, BonesResource bones)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(bones.BoneNameHashes.Length);
        writer.Write(bones.Lods.Length);
        for (int i = 0; i < bones.BoneNameHashes.Length; i++)
            writer.Write(bones.BoneNameHashes[i]);
        for (int i = 0; i < bones.Lods.Length; i++)
            writer.Write(bones.Lods[i]);
        for (int i = 0; i < bones.BoneNameHashes.Length; i++)
            writer.WriteNullTermString(bones.BoneNames[i]);
    }
}