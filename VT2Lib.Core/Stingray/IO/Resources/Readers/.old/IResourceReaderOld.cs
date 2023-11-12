using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

public interface IResourceReaderOld
{
    bool CanRead(IDString64 resourceType);

    bool CanRead<TResource>() where TResource : IResource;

    bool CanRead(Type? resourceType);

    IResource Read(Stream stream);
}
