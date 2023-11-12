using VT2Lib.Core.Stingray.IO.Resources.Writers;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers.SJson;

public interface ISJsonResourceWriter : IResourceWriter
{
    void Write(StreamWriter writer, IResource resource);
}