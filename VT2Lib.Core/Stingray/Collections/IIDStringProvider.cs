namespace VT2Lib.Core.Stingray.Collections;

/// <summary>
/// Provides an interface for retrieving IDStrings from ID hash values.
/// </summary>
public interface IIDStringProvider : IIDString32Provider, IIDString64Provider
{
}