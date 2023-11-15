using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers;

public sealed class VersionedResourceWriterRepository
{
    private readonly ConcurrentDictionary<int, ResourceWriterDelegate> _versionWriterRepo = new();

    public bool HasReaderForVersion(int resourceVersion)
    {
        return _versionWriterRepo.ContainsKey(resourceVersion);
    }

    public bool TryRegister(int resourceVersion, ResourceWriterDelegate writerFunc)
    {
        return _versionWriterRepo.TryAdd(resourceVersion, writerFunc);
    }

    public bool TryUnregister(int resourceVersion)
    {
        return _versionWriterRepo.TryRemove(resourceVersion, out _);
    }

    public bool TryGet(int resourceVersion, [NotNullWhen(true)] out ResourceWriterDelegate? writerFunc)
    {
        return _versionWriterRepo.TryGetValue(resourceVersion, out writerFunc);
    }
}