using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

public class ResourceReader : IResourceReader
{
    public IDString64 ResourceID { get; }

    protected readonly ResourceReaderDelegate _readerFunc;

    public ResourceReader(IDString64 resourceID, ResourceReaderDelegate readerFunc)
    {
        ResourceID = resourceID;
        _readerFunc = readerFunc;
    }

    public bool CanRead(IDString64 resourceID)
    {
        return resourceID == ResourceID;
    }

    public IResource Read(Stream stream)
    {
        var reader = new PrimitiveReader(stream);
        return _readerFunc(in reader);
    }
}