namespace VT2Lib.Core.IO.Serialization;

internal interface IAsyncSerializer<T>
{
    ValueTask SerializeAsync(Stream stream, T value, CancellationToken cancellationToken = default);

    ValueTask<T> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default);
}