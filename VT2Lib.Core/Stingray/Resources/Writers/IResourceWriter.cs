namespace VT2Lib.Core.Stingray.Resources.Writers;

public interface IResourceWriter
{
    bool CanWrite(IDString64 resourceType);

    bool CanWrite(Type? resourceType);

    bool CanWrite<TResource>() where TResource : IResource;

    void Write(Stream stream, IResource resource);
}