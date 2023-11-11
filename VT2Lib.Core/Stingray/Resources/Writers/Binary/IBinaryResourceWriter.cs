namespace VT2Lib.Core.Stingray.Resources.Writers.Binary;

public interface IBinaryResourceWriter : IResourceWriter
{
    void Write(BinaryWriter writer, IResource resource);
}