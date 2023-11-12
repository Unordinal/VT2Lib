using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers;

public interface IResourceWriter
{
    void Write(Stream stream, IResource resource);
}