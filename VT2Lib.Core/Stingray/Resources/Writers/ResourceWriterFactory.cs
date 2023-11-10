using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace VT2Lib.Core.Stingray.Resources.Writers;

public sealed class ResourceWriterFactory
{
    public static ResourceWriterFactory Shared { get; }

    private readonly ConcurrentDictionary<IDString64, Func<IResourceWriter>> _writerFactories = new();

    static ResourceWriterFactory()
    {
        Shared = new ResourceWriterFactory();
        Shared.RegisterWriter(BonesResource.ResourceType, () => new BonesResourceWriter());
    }

    public IResourceWriter CreateWriter(IDString64 resourceType)
    {
        if (!_writerFactories.TryGetValue(resourceType, out var writerFactory))
            throw new ArgumentException($"No writer registered for resource type '{resourceType}'");

        return writerFactory();
    }

    public void RegisterWriter(IDString64 resourceType, Func<IResourceWriter> writerFactory)
    {
        ArgumentNullException.ThrowIfNull(writerFactory);

        if (!writerFactory().CanWrite(resourceType))
            throw new ArgumentException($"The given writer cannot write the resource type '{resourceType}' it was registered for.");

        if (!_writerFactories.TryAdd(resourceType, writerFactory))
            throw new ArgumentException($"A writer is already registered for resource type '{resourceType}'");
    }

    public bool UnregisterWriter(IDString64 resourceType, [NotNullWhen(true)] out Func<IResourceWriter>? writerFactory)
    {
        return _writerFactories.TryRemove(resourceType, out writerFactory);
    }
}