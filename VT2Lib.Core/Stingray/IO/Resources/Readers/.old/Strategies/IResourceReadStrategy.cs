using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers.Strategies;

internal interface IResourceReadStrategy
{
    IResource Read(Stream stream);
}
