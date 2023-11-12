using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.IO.Resources.Readers.Strategies;
using VT2Lib.Core.Stingray.IO.Resources.Readers.Strategies.Bones;
using VT2Lib.Core.Stingray.Resources;
using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

internal static class BinaryResourceReadStrategies
{
    public static Dictionary<int, Func<Stream, IResource>> Bones = new()
    {
        [0] = (stream) =>
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
    };

    public static Dictionary<int, Func<IResourceReadStrategy>> BonesR = new()
    {
        [0] = () => new BonesResourceV1ReadStrategy()
    };

    public static Dictionary<int, Func<Stream, IResource>> BonesN = new()
    {
        [0] = (stream) => new BonesResourceV1ReadStrategy().Read(stream)
    };
}
