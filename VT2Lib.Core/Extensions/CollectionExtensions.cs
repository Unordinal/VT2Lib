using VT2Lib.Core.Collections;

namespace VT2Lib.Core.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Gets a read-only wrapper over the given <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values the collection holds.</typeparam>
    /// <param name="collection">The collection to return a read-only wrapper over.</param>
    /// <returns>A read-only collection wrapper over <paramref name="collection"/>.</returns>
    public static IReadOnlyCollection<T> AsReadOnlyEx<T>(this ICollection<T> collection)
    {
        return new ReadOnlyCollection<T>(collection);
    }
}