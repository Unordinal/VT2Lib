using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

// Okay, I may be going a little overboard? There must be a better, simpler, less verbose way to do this.
// The goal is to allow external code to add their own readers for both resources and versions of those resources.
public sealed class VersionedResourceReaderRepository
{
    private readonly ConcurrentDictionary<int, ResourceReaderDelegate> _versionReaderRepo = new();

    public bool HasReaderForVersion(int resourceVersion)
    {
        return _versionReaderRepo.ContainsKey(resourceVersion);
    }

    public bool TryRegister(int resourceVersion, ResourceReaderDelegate readerFunc)
    {
        return _versionReaderRepo.TryAdd(resourceVersion, readerFunc);
    }

    public bool TryUnregister(int resourceVersion)
    {
        return _versionReaderRepo.TryRemove(resourceVersion, out _);
    }

    public bool TryGet(int resourceVersion, [NotNullWhen(true)] out ResourceReaderDelegate? readerFunc)
    {
        return _versionReaderRepo.TryGetValue(resourceVersion, out readerFunc);
    }
}