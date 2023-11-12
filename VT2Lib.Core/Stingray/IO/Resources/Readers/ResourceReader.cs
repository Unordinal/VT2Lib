using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

public abstract class ResourceReader<TResource> : IResourceReader
    where TResource : IResource
{
    public abstract TResource Read(Stream stream);

    IResource IResourceReader.Read(Stream stream)
    {
        return Read(stream);
    }

    public static implicit operator ResourceReaderDelegate(ResourceReader<TResource> reader) => ((IResourceReader)reader).Read;
}