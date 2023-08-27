using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Core.Extensions;
using VT2Lib.Core.Stingray.Resources.Readers;

namespace VT2Lib.Core.Stingray.Resources.Writers;

public sealed class BonesResourceWriter : ResourceWriter<BonesResource>
{
    public override void Write(BinaryWriter writer, BonesResource bones)
    {
        ArgumentNullException.ThrowIfNull(writer);
        Debug.Assert(bones.NumBones == bones.BoneNameHashes.Length);
        Debug.Assert(bones.NumLodLevels == bones.Lods.Length);
        Debug.Assert(bones.NumBones == bones.BoneNames.Length);

        writer.Write(bones.NumBones);
        writer.Write(bones.NumLodLevels);
        for (int i = 0; i < bones.NumBones; i++)
            writer.Write(bones.BoneNameHashes[i]);
        for (int i = 0; i < bones.NumLodLevels; i++)
            writer.Write(bones.Lods[i]);
        for (int i = 0; i < bones.NumBones; i++)
            writer.WriteNullTermString(bones.BoneNames[i]);
    }
}
