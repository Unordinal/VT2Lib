using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

public interface IResourceReader
{
    IResource Read(Stream stream);
}