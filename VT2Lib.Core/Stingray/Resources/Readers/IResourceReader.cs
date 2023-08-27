using VT2Lib.Core.IO;

namespace VT2Lib.Core.Stingray.Resources.Readers;

public interface IResourceReader
{
    bool CanRead(IDString64 resourceType);

    bool CanRead<TResource>() where TResource : IResource;

    bool CanRead(Type? resourceType);

    IResource Read(PrimitiveReader reader);

    IResource Read(ReadOnlySpan<byte> buffer);
}