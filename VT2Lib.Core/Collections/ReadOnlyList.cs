using System.Collections;

namespace VT2Lib.Core.Collections;

internal class ReadOnlyList<T> : IReadOnlyList<T>
{
    public int Count => _list.Count;

    public T this[int index] => _list[index];

    private readonly IList<T> _list;

    public ReadOnlyList(IList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        _list = list;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}