namespace VT2Lib.Core.Stingray.Resources.Writers;

public abstract class ResourceWriter<TResource> : IResourceWriter
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

    void IResourceWriter.Write(BinaryWriter writer, IResource resource)
    {
        var resourceType = resource.GetType();
        if (resourceType != typeof(TResource))
            throw new ArgumentException($"'{GetType().Name}' does not support the resource type '{resourceType}'.");

        Write(writer, (TResource)resource);
    }
}