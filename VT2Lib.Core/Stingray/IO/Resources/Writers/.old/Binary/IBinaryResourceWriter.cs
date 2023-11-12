using VT2Lib.Core.Stingray.IO.Resources.Writers;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers.Binary;

public interface IBinaryResourceWriter : IResourceWriter
{
    void Write(BinaryWriter writer, IResource resource);
}