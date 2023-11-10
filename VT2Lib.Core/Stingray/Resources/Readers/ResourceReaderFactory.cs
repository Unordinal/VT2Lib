using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace VT2Lib.Core.Stingray.Resources.Readers;

// EVAL: Is holding and reusing a single reader instance per type a good idea vs.
// having a factory Func<IResourceReader> which creates new instances when needed?
// A single instance may have adverse effects if a reader has state and is used from multiple threads. (Though readers should probably be stateless.)
// Should figure this out sooner rather than later since it'll change the RegisterReader parameters.
// Having a Func<> makes it possible to create a copy of an existing reader if needed.
public sealed class ResourceReaderFactory
{
    public static ResourceReaderFactory Shared { get; }

    private readonly ConcurrentDictionary<IDString64, Func<IResourceReader>> _readerFactories = new();

    static ResourceReaderFactory()
    {
        Shared = new ResourceReaderFactory();
        Shared.RegisterReader(BonesResource.ResourceType, () => new BonesResourceReader());
    }

    public IResourceReader CreateReader(IDString64 resourceType)
    {
        if (!_readerFactories.TryGetValue(resourceType, out var readerFactory))
            throw new ArgumentException($"No reader registered for resource type '{resourceType}'");

        return readerFactory();
    }

    public void RegisterReader(IDString64 resourceType, Func<IResourceReader> readerFactory)
    {
        ArgumentNullException.ThrowIfNull(readerFactory);
        if (!readerFactory().CanRead(resourceType))
            throw new ArgumentException($"The given reader cannot read the resource type '{resourceType}' it was registered for.");

        if (!_readerFactories.TryAdd(resourceType, readerFactory))
            throw new ArgumentException($"A reader is already registered for resource type '{resourceType}'");
    }

    public bool UnregisterReader(IDString64 resourceType, [NotNullWhen(true)] out Func<IResourceReader>? readerFactory)
    {
        return _readerFactories.TryRemove(resourceType, out readerFactory);
    }
}