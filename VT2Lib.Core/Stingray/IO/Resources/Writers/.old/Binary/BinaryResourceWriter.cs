using System.Security.AccessControl;
using VT2Lib.Core.Stingray.IO.Resources.Writers;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers.Binary;

public abstract class BinaryResourceWriter<TResource> : IBinaryResourceWriter
    where TResource : IResource
{
    public bool CanWrite(IDString64 resourceType)
    {
        return TResource.ResourceType == resourceType;
    }

    public bool CanWrite<T>() where T : IResource
    {
        return typeof(TResource) == typeof(T);
    }

    public bool CanWrite(Type? resourceType)
    {
        return typeof(TResource) == resourceType;
    }

    public abstract void Write(BinaryWriter writer, TResource resource);

    /// <summary>
    /// Ensures that the resource is not <see langword="null"/> and the type of the resource is compatible
    /// with this <see cref="BinaryResourceWriter{TResource}"/>.
    /// <br/>
    /// Throws if either of these conditions are <see langword="false"/>.
    /// </summary>
    /// <param name="resource"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    protected void EnsureCanWriteResource(IResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        Type resourceType = resource.GetType();
        if (!CanWrite(resourceType))
            throw new ArgumentException($"'{GetType().Name}' does not support the resource type '{resourceType}'.");
    }

    void IBinaryResourceWriter.Write(BinaryWriter writer, IResource resource)
    {
        ArgumentNullException.ThrowIfNull(writer);
        EnsureCanWriteResource(resource);

        Write(writer, (TResource)resource);
    }

    void IResourceWriter.Write(Stream stream, IResource resource)
    {
        ArgumentNullException.ThrowIfNull(stream);
        EnsureCanWriteResource(resource);

        using var writer = new BinaryWriter(stream);
        Write(writer, (TResource)resource);
    }
}