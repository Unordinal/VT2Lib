namespace VT2Lib.Core.Stingray.Resources;

public abstract class Resource<TResource> : IResource
    where TResource : IResource
{
    public abstract IDString64 ResourceID { get; }

    private int? _version;

    public int GetResourceVersion()
    {
        _version ??= ResourceVersionUtil.GetResourceVersion(GetType());
        return _version.Value;
    }

    public override string ToString()
    {
        return $"<{GetType().Name} ('{ResourceID}' {GetResourceVersion()})>";
    }
}