using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers;

public interface IResourceWriter
{
    IDString64 ResourceID { get; }

    bool CanWrite(IDString64 resourceID);

    void Write(Stream stream, IResource resource);
}