using VT2Lib.Core.Stingray.Resources.Bones;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers.Bones;

public class BinaryBonesResourceReader : ResourceReaderOld<BonesResource>
{
    public override BonesResource Read(Stream stream)
    {
        // With versioned resources, switch on resource version and choose appropriate resource version's reading method.
        return BonesResourceV0.ReadBinary(stream);
    }
}