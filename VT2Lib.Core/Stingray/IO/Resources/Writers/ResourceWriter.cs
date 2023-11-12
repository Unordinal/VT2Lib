using System.Diagnostics;
using System.Reflection;
using VT2Lib.Core.Stingray.Attributes;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers;

public abstract class ResourceWriter<TResource> : IResourceWriter
    where TResource : IResource
{
    public abstract void Write(Stream stream, TResource resource);

    public virtual void Write(Stream stream, IResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ValidateResourceType(resource);
        Write(stream, (TResource)resource);
    }

    protected void ValidateResourceType(IResource resource)
    {
        var resourceType = resource.GetResourceID();
        if (resourceType != TResource.ResourceID)
            throw new ArgumentException($"{nameof(ResourceWriter<TResource>)} of type '{TResource.ResourceID}' cannot write a resource of type '{resourceType}'");
    }

    public static implicit operator ResourceWriterDelegate(ResourceWriter<TResource> writer) => writer.Write;
}