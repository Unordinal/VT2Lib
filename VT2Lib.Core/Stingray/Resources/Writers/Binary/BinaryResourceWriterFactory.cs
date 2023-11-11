using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace VT2Lib.Core.Stingray.Resources.Writers.Binary;

public sealed class BinaryResourceWriterFactory
{
    public static BinaryResourceWriterFactory Shared { get; }

    private readonly ConcurrentDictionary<IDString64, Func<IBinaryResourceWriter>> _writerFactories = new();

    static BinaryResourceWriterFactory()
    {
        Shared = new BinaryResourceWriterFactory();
        Shared.RegisterWriter(BonesResource.ResourceType, () => new BonesBinaryResourceWriter());
    }

    public IBinaryResourceWriter CreateWriter(IDString64 resourceType)
    {
        if (!_writerFactories.TryGetValue(resourceType, out var writerFactory))
            throw new ArgumentException($"There is no writer registered for the resource type '{resourceType}'");

        return writerFactory();
    }

    public void RegisterWriter(IDString64 resourceType, Func<IBinaryResourceWriter> writerFactory)
    {
        ArgumentNullException.ThrowIfNull(writerFactory);

        if (!writerFactory().CanWrite(resourceType))
            throw new ArgumentException($"The given writer cannot write the specified resource type '{resourceType}'.");

        if (!_writerFactories.TryAdd(resourceType, writerFactory))
            throw new ArgumentException($"A writer is already registered for the specified resource type '{resourceType}'");
    }

    public bool UnregisterWriter(IDString64 resourceType, [NotNullWhen(true)] out Func<IBinaryResourceWriter>? writerFactory)
    {
        return _writerFactories.TryRemove(resourceType, out writerFactory);
    }
}