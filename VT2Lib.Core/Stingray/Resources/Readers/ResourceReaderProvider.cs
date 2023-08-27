using System.Collections.Concurrent;

namespace VT2Lib.Core.Stingray.Resources.Readers;

// EVAL: Is holding and reusing a single reader instance per type a good idea vs.
// having a factory Func<IResourceReader> which creates new instances when needed?
// May have adverse effects if a reader has state and is used from multiple threads. (Though readers should probably be stateless.)
// Should figure this out sooner rather than later since it'll change the RegisterReader parameters.
// Having a Func<> makes it possible to create a copy of an existing provider if needed.
public sealed class ResourceReaderProvider
{
    public static ResourceReaderProvider Shared { get; } = new();

    private readonly ConcurrentDictionary<IDString64, IResourceReader> _readerMap = new();

    public IResourceReader GetReaderForType(IDString64 resourceType)
    {
        if (!_readerMap.TryGetValue(resourceType, out var reader))
            throw new ArgumentException($"No reader registered for resource type '{resourceType}'");

        return reader;
    }

    public void RegisterReader(IDString64 resourceType, IResourceReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (!reader.CanRead(resourceType))
            throw new ArgumentException($"The given reader cannot read the resource type '{resourceType}' it was registered for.");

        if (!_readerMap.TryAdd(resourceType, reader))
            throw new ArgumentException($"A reader is already registered for resource type '{resourceType}'");
    }

    public bool UnregisterReader(IDString64 resourceType)
    {
        return _readerMap.TryRemove(resourceType, out var _);
    }
}