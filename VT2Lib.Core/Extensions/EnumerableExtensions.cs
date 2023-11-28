namespace VT2Lib.Core.Extensions;

public static class EnumerableExtensions
{
    public static int IndexOf<T>(this IEnumerable<T> values, T item)
    {
        return values.IndexOf(item, EqualityComparer<T>.Default);
    }

    public static int IndexOf<T>(this IEnumerable<T> values, T item, IEqualityComparer<T>? comparer)
    {
        comparer ??= EqualityComparer<T>.Default;
        return values.IndexOf((elem) => comparer.Equals(item, elem));
    }

    public static int IndexOf<T>(this IEnumerable<T> values, Func<T, bool> predicate)
    {
        int index = 0;
        foreach (var elem in values)
        {
            if (predicate(elem))
                return index;

            index++;
        }

        return -1;
    }
}