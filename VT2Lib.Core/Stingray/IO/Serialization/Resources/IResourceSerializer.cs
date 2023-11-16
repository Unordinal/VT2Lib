using VT2Lib.Core.IO.Serialization;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources;

public interface IResourceSerializer : ISerializer<IResource>
{
    IDString64 ResourceID { get; }

    public TResource DeserializeAs<TResource>(Stream stream) where TResource : IResource
    {
        return (TResource)Deserialize(stream);
    }
}