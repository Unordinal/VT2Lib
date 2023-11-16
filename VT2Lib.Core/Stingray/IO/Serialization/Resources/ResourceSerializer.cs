using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources;

public abstract class ResourceSerializer<TResource> : IResourceSerializer
    where TResource : IResource
{
    public IDString64 ResourceID { get; }

    protected ResourceSerializer(IDString64 resourceID)
    {
        ResourceID = resourceID;
    }

    public abstract void Serialize(Stream stream, TResource resource);

    public abstract TResource Deserialize(Stream stream);

    void ISerializer<IResource>.Serialize(Stream stream, IResource value)
    {
        if (value is not TResource resource)
            throw new ArgumentException(
                $"{GetType().Name} of type '{ResourceID} cannot serialize a {value.GetType().Name} resource of type '{value.ResourceID}'");

        Serialize(stream, resource);
    }

    IResource ISerializer<IResource>.Deserialize(Stream stream)
    {
        return Deserialize(stream);
    }
}