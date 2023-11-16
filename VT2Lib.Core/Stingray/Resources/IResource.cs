using VT2Lib.Core.Stingray.Attributes;

namespace VT2Lib.Core.Stingray.Resources;

// EVAL: Should this be named something like IStingrayResource instead?
/// <summary>
/// Represents a Stingray resource.
/// </summary>
public interface IResource
{
    /// <summary>
    /// Gets the type of resource this is as an <see cref="IDString64"/>.
    /// </summary>
    IDString64 ResourceID { get; }

    /// <summary>
    /// Gets the version of this resource. 
    /// Returns <see cref="StingrayResourceAttribute.Versionless"/> if the resource does not have a specified version.
    /// </summary>
    /// <returns>The resource version.</returns>
    int GetResourceVersion();
}