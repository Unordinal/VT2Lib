using VT2Lib.Core.IO;

namespace VT2Lib.Core.Stingray.Resources.Readers.Binary;

public interface IBinaryResourceReader : IResourceReader
{
    IResource Read(PrimitiveReader reader);

    IResource Read(ReadOnlySpan<byte> buffer);
}