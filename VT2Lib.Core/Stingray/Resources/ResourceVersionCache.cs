using System.Collections.Concurrent;
using System.Reflection;
using VT2Lib.Core.Stingray.Attributes;

namespace VT2Lib.Core.Stingray.Resources;

internal static class ResourceVersionCache
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
}