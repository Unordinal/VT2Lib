namespace VT2Lib.Core.Stingray.Collections.Extensions;

public static class IDStringProviderExtensions
{
    /// <summary>
    /// Attempts to get the <see cref="IDString32"/> with the specified ID from this <see cref="IIDString32Provider"/>,
    /// creating and returning a new <see cref="IDString32"/> if one is not found.
    /// </summary>
    /// <param name="provider">The ID string provider to use.</param>
    /// <param name="id">The ID of the value to lookup.</param>
    /// <returns>The <see cref="IDString32"/> with the specified <paramref name="id"/> if one was found; otherwise, a new <see cref="IDString32"/> with the specified <paramref name="id"/>.</returns>
    public static IDString32 GetOrCreate(this IIDString32Provider provider, uint id)
    {
        if (provider.TryGet(id, out var result))
            return result;

        return new IDString32(id);
    }

    /// <summary>
    /// Attempts to get the <see cref="IDString64"/> with the specified ID from this <see cref="IIDString64Provider"/>,
    /// creating and returning a new <see cref="IDString64"/> if one is not found.
    /// </summary>
    /// <param name="provider">The ID string provider to use.</param>
    /// <param name="id">The ID of the value to lookup.</param>
    /// <returns>The <see cref="IDString32"/> with the specified <paramref name="id"/> if one was found; otherwise, a new <see cref="IDString64"/> with the specified <paramref name="id"/>.</returns>
    public static IDString64 GetOrCreate(this IIDString64Provider provider, ulong id)
    {
        if (provider.TryGet(id, out var result))
            return result;

        return new IDString64(id);
    }
}