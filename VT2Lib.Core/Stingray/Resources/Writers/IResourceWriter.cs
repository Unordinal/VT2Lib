namespace VT2Lib.Core.Stingray.Resources.Writers;

public interface IResourceWriter
{
    bool CanWrite(IDString64 resourceType);

    bool CanWrite<TResource>() where TResource : IResource;

    bool CanWrite(Type? resourceType);

    void Write(BinaryWriter writer, IResource resource);
}