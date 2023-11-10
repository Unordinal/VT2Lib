using VT2Lib.Core.Collections;

namespace VT2Lib.Core.Extensions;

internal static class ListExtensions
{
    /// <summary>
    /// Gets a read-only wrapper over the given <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values the list holds.</typeparam>
    /// <param name="list">The list to return a read-only wrapper over.</param>
    /// <returns>A read-only list wrapper over <paramref name="list"/>.</returns>
    public static IReadOnlyList<T> AsReadOnlyEx<T>(this IList<T> list)
    {
        return new ReadOnlyList<T>(list);
    }
}