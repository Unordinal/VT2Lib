using System.Runtime.InteropServices;

namespace VT2Lib.Core.Stingray;

/// <summary>
/// Represents a resource's type and path.
/// </summary>
/// <param name="Type"> Gets the type of the resource. </param>
/// <param name="Name"> Gets the path of the resource. </param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct ResourceLocator(IDString64 Type, IDString64 Name);