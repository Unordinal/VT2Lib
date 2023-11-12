using System.Reflection;
using VT2Lib.Core.Stingray.Attributes;

namespace VT2Lib.Core.Stingray.Resources;

public abstract record class Resource<TResource> : IResource
    where TResource : IResource
{
    public IDString64 GetResourceID()
    {
        return TResource.ResourceID;
    }

    public int GetResourceVersion()
    {
        var versionAttr = GetType().GetCustomAttributes<ResourceVersionAttribute>();
        return versionAttr.FirstOrDefault()?.Version ?? throw new InvalidOperationException($"Resource has no declared {nameof(ResourceVersionAttribute)}");
    }
}