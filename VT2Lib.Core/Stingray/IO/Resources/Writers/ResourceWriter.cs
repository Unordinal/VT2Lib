using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers;

public class ResourceWriter : IResourceWriter
{
    public IDString64 ResourceID { get; }

    private readonly ResourceWriterDelegate _writerFunc;

    public ResourceWriter(IDString64 resourceID, ResourceWriterDelegate writerFunc)
    {
        ResourceID = resourceID;
        _writerFunc = writerFunc;
    }

    public bool CanWrite(IDString64 resourceID)
    {
        return resourceID == ResourceID;
    }

    public virtual void Write(Stream stream, IResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ValidateResourceType(resource);

        var writer = new PrimitiveWriter(stream);
        _writerFunc(in writer, resource);
    }

    protected void ValidateResourceType(IResource resource)
    {
        var resourceType = resource.GetResourceID();
        if (resourceType != ResourceID)
            throw new ArgumentException($"{nameof(ResourceWriter)} of type '{ResourceID}' cannot write a resource of type '{resourceType}'.");
    }
}