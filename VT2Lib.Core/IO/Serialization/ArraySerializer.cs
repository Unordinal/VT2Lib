using VT2Lib.Core.Extensions;
using VT2Lib.Core.Stingray.IO.Serialization.Resources;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.IO.Serialization;

internal sealed class ArraySerializer<T> : SerializerBase<T[]>
{
    private readonly Action<Stream, T> _serializer;
    private readonly Func<Stream, T> _deserializer;

    internal ArraySerializer(Action<Stream, T> serializer, Func<Stream, T> deserializer)
    {
        _serializer = serializer;
        _deserializer = deserializer;
    }

    public override void Serialize(Stream stream, T[] value)
    {
        stream.WriteInt32LE(value.Length);
        for (int i = 0; i < value.Length; i++)
            _serializer(stream, value[i]);
    }

    public void Serialize(Stream stream, scoped ReadOnlySpan<T> value)
    {
        stream.WriteInt32LE(value.Length);
        for (int i = 0; i < value.Length; i++)
            _serializer(stream, value[i]);
    }

    public override T[] Deserialize(Stream stream)
    {
        int length = stream.ReadInt32LE();
        T[] array = new T[length];
        for (int i = 0; i < length; i++)
            array[i] = _deserializer(stream);

        return array;
    }
}

internal static class ArraySerializer
{
    public static ArraySerializer<T> Create<T>(ISerializer<T> serializer)
    {
        return new ArraySerializer<T>(serializer.Serialize, serializer.Deserialize);
    }

    public static ArraySerializer<T> Create<T>(ResourceSerializer<T> serializer)
        where T : IResource
    {
        return new ArraySerializer<T>(serializer.Serialize, serializer.Deserialize);
    }

    public static ArraySerializer<T> Create<T>()
        where T : ISerializable<T>
    {
        return new ArraySerializer<T>(T.Serialize, T.Deserialize);
    }
}