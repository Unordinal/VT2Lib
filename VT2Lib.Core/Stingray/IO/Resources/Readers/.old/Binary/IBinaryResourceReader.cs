using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.IO.Resources.Readers;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers.Binary;

public interface IBinaryResourceReader : IResourceReaderOld
{
    IResource Read(PrimitiveReader reader);

    IResource Read(ReadOnlySpan<byte> buffer);
}