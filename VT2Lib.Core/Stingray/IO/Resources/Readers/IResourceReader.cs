using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

public interface IResourceReader
{
    IDString64 ResourceID { get; }

    bool CanRead(IDString64 resourceID);

    IResource Read(Stream stream);
}