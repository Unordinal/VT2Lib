using System.Reflection;
using VT2Lib.Core.Stingray.Attributes;

namespace VT2Lib.Core.Stingray.Resources;

public abstract class Resource<TResource> : IResource
    where TResource : IResource
{
    private static IEnumerable<(Type, int)>? _allVersions;

    private readonly int _version;

    public Resource()
    {
        _version = ResourceVersionUtil.GetResourceVersion(GetType());
    }

    public IDString64 GetResourceID()
    {
        return TResource.ResourceID;
    }

    public int GetResourceVersion()
    {
        return _version;
    }

    /*public static IEnumerable<(Type Type, int Version)> GetAllVersions()
    {
        _allVersions ??= ReflectionUtil.GetAllSubclasses(typeof(Resource<TResource>))
                .Where(t => t.IsDefined(typeof(StingrayResourceAttribute), false))
                .Select(t => (Type: t, t.GetCustomAttribute<StingrayResourceAttribute>(false)!.Version));

        return _allVersions;
    }*/
}