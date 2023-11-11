using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VT2Lib.Core.Stingray.Resources.Readers;

public interface IResourceReader
{
    bool CanRead(IDString64 resourceType);

    bool CanRead<TResource>() where TResource : IResource;

    bool CanRead(Type? resourceType);

    IResource Read(Stream stream);
}
