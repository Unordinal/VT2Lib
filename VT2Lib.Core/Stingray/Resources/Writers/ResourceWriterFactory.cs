using System.Collections.Concurrent;

namespace VT2Lib.Core.Stingray.Resources.Writers;

public sealed class ResourceWriterFactory
{
    public static ResourceWriterFactory Shared { get; } = new();

    private readonly ConcurrentDictionary<IDString64, Func<IResourceWriter>> _factories = new();

    public IResourceWriter Create(IDString64 resourceType)
    {
        if (!_factories.TryGetValue(resourceType, out var factory))
            throw new ArgumentException($"No writer factory registered for resource type '{resourceType}'");

        return factory();
    }

    public void RegisterFactory(IDString64 resourceType, Func<IResourceWriter> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (!_factories.TryAdd(resourceType, factory))
            throw new ArgumentException($"A writer factory is already registered for resource type '{resourceType}'");
    }
}