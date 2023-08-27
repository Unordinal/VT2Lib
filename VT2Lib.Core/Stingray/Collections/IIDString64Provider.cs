namespace VT2Lib.Core.Stingray.Collections;

public interface IIDString64Provider
{
    /// <inheritdoc cref="Get(ulong)"/>
    ///
    public IDString64 this[ulong id] => Get(id);

    /// <summary>
    /// Gets whether this provider contains an <see cref="IDString64"/> with the specified hash.
    /// </summary>
    /// <param name="id">The ID to lookup.</param>
    /// <returns><see langword="true"/> if this <see cref="IIDStringProvider"/> contains the specified hash value; otherwise, <see langword="false"/>.</returns>
    bool ContainsID(ulong id);

    /// <summary>
    /// Gets the <see cref="IDString64"/> with the specified ID from this <see cref="IIDStringProvider"/>.
    /// </summary>
    /// <param name="id">The ID of the value to lookup.</param>
    /// <returns>The <see cref="IDString64"/> retrieved from the provider.</returns>
    IDString64 Get(ulong id);

    /// <summary>
    /// Attempts to get the <see cref="IDString64"/> with the specified ID from this <see cref="IIDStringProvider"/>.
    /// </summary>
    /// <param name="id">The ID of the value to lookup.</param>
    /// <param name="result">If this method returns <see langword="true"/>, the <see cref="IDString64"/> that was found; otherwise, <see langword="default"/>(<see cref="IDString64"/>).</param>
    /// <returns><see langword="true"/> if an <see cref="IDString64"/> with the specified <paramref name="id"/> was found; otherwise, <see langword="false"/>.</returns>
    bool TryGet(ulong id, out IDString64 result);
}