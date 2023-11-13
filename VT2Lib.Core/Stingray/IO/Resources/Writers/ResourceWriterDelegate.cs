using VT2Lib.Core.IO;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Writers;

public delegate void ResourceWriterDelegate(in PrimitiveWriter writer, IResource resource);