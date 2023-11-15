namespace VT2Lib.Core.IO.Serialization;

public abstract class SerializerBase<T> : ISerializer<T>
{
    public abstract void Serialize(Stream stream, T value);

    public abstract T Deserialize(Stream stream);
}