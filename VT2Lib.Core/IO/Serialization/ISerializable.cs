namespace VT2Lib.Core.IO.Serialization;

internal interface ISerializable<T>
{
    /// <summary>
    /// Serializes a <typeparamref name="T"/> to the given stream.
    /// </summary>
    /// <param name="stream">The stream to write <paramref name="value"/> to.</param>
    /// <param name="value">The value of type <typeparamref name="T"/> to write to <paramref name="stream"/>.</param>
    static abstract void Serialize(Stream stream, T value);

    /// <summary>
    /// Deserializes a <typeparamref name="T"/> from the given stream.
    /// </summary>
    /// <param name="stream">The stream to read a value of type <typeparamref name="T"/> from.</param>
    /// <returns>The value of type <typeparamref name="T"/> read from <paramref name="stream"/>.</returns>
    static abstract T Deserialize(Stream stream);
}