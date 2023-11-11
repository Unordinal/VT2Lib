using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace VT2Lib.Core.Stingray.Resources.Readers.Binary;

// EVAL: Is holding and reusing a single reader instance per type a good idea vs.
// having a factory Func<IResourceReader> which creates new instances when needed?
// A single instance may have adverse effects if a reader has state and is used from multiple threads. (Though readers should probably be stateless.)
// Should figure this out sooner rather than later since it'll change the RegisterReader parameters.
// Having a Func<> makes it possible to create a copy of an existing reader if needed.
public sealed class BinaryResourceReaderFactory
{
    public static BinaryResourceReaderFactory Shared { get; }

    private readonly ConcurrentDictionary<IDString64, Func<IBinaryResourceReader>> _readerFactories = new();

    static BinaryResourceReaderFactory()
    {
        Shared = new BinaryResourceReaderFactory();
        Shared.RegisterReader(BonesResource.ResourceType, () => new BonesBinaryResourceReader());
    }

    public IBinaryResourceReader CreateReader(IDString64 resourceType)
    {
        if (!_readerFactories.TryGetValue(resourceType, out var readerFactory))
            throw new ArgumentException($"There is no reader registered for the resource type '{resourceType}'");

        return readerFactory();
    }

    public void RegisterReader(IDString64 resourceType, Func<IBinaryResourceReader> readerFactory)
    {
        ArgumentNullException.ThrowIfNull(readerFactory);
        if (!readerFactory().CanRead(resourceType))
            throw new ArgumentException($"The given reader cannot read the specified resource type '{resourceType}'.");

        if (!_readerFactories.TryAdd(resourceType, readerFactory))
            throw new ArgumentException($"A reader is already registered for the specified resource type '{resourceType}'");
    }

    public bool UnregisterReader(IDString64 resourceType, [NotNullWhen(true)] out Func<IBinaryResourceReader>? readerFactory)
    {
        return _readerFactories.TryRemove(resourceType, out readerFactory);
    }
}