using System.Collections;

namespace VT2Lib.Core.Collections;

internal class ReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    public int Count => _collection.Count;

    private readonly ICollection<T> _collection;

    public ReadOnlyCollection(ICollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        _collection = collection;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}