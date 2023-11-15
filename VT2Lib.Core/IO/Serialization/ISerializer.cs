namespace VT2Lib.Core.IO.Serialization;

internal interface ISerializer<T>
{
    void Serialize(Stream stream, T value);

    T Deserialize(Stream stream);
}