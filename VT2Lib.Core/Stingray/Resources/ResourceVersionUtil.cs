using System.Collections.Concurrent;
using System.Reflection;
using VT2Lib.Core.Stingray.Attributes;

namespace VT2Lib.Core.Stingray.Resources;

internal static class ResourceVersionUtil
{
    private static readonly ConcurrentDictionary<Type, int> _versionCache = new();

    public static int GetResourceVersion(Type resourceType)
    {
        if (!_versionCache.TryGetValue(resourceType, out int version))
        {
            if (!resourceType.IsAssignableTo(typeof(IResource)))
                throw new ArgumentException("The given type is not an IResource.");

            var versionAttr = resourceType.GetCustomAttribute<StingrayResourceAttribute>();
            version = versionAttr?.Version ?? StingrayResourceAttribute.Versionless;

            _versionCache.TryAdd(resourceType, version);
        }

        return version;
    }

    public static void EnsureSupportedVersion<TResource>(int resourceVersion)
        where TResource : IResource
    {
        if (!IsSupportedVersion<TResource>(resourceVersion))
            throw new InvalidDataException($"Unsupported resource version '{resourceVersion}' for type '{typeof(TResource).Name}'");
    }

    public static bool IsSupportedVersion<TResource>(int resourceVersion)
        where TResource : IResource
    {
        int version = GetResourceVersion(typeof(TResource));
        return version == resourceVersion;
    }
}