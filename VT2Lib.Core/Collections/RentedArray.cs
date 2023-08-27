using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace VT2Lib.Core.Collections;

/// <summary>
/// Provides easy renting and returning of an array from/to an <see cref="ArrayPool{T}"/>.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
///     <see langword="using"/> <see cref="RentedArray{T}"/> rented = <see langword="new"/>(length); // Optional <see cref="ArrayPool{T}"/> argument
/// </code>
/// </remarks>
/// <typeparam name="T"></typeparam>
public sealed class RentedArray<T> : IDisposable
{
    /// <summary>
    /// Gets a span over the rented array of exactly the requested length.
    /// </summary>
    public Span<T> Span
    {
        get
        {
            ThrowIfDisposed();
            return _rented.AsSpan(0, _length);
        }
    }

    /// <summary>
    /// Gets a new array segment over the rented array of exactly the requested length.
    /// </summary>
    public ArraySegment<T> Segment
    {
        get
        {
            ThrowIfDisposed();
            return new ArraySegment<T>(_rented, 0, Length);
        }
    }

    /// <summary>
    /// Returns the raw, rented array. The array's length may be longer than the originally requested length.
    /// </summary>
    /// <remarks>
    /// References to the array returned by this should be discarded and not used once this instance is disposed,
    /// as the rented array will have been returned to the source array pool.
    /// </remarks>
    public T[] RawArrayUnsafe
    {
        get
        {
            ThrowIfDisposed();
            return _rented;
        }
    }

    /// <summary>
    /// Gets the originally requested length of the rented array.
    /// </summary>
    public int Length => _length;

    private readonly int _length;
    private readonly ArrayPool<T> _pool;
    private T[]? _rented;

    public RentedArray(int length) : this(length, ArrayPool<T>.Shared)
    {
    }

    public RentedArray(int length, ArrayPool<T> pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        _length = length;
        _pool = pool;
        _rented = _pool.Rent(length);
    }

    public Span<T> AsSpan() => Span;

    public Span<T> AsSpan(int start)
    {
        ThrowIfDisposed();
        return _rented.AsSpan(start);
    }

    public Span<T> AsSpan(int start, int length)
    {
        ThrowIfDisposed();
        return _rented.AsSpan(start, length);
    }

    public Memory<T> AsMemory() => new(RawArrayUnsafe, 0, _length);

    public Memory<T> AsMemory(int start) => new(RawArrayUnsafe, start, _length - start);

    public Memory<T> AsMemory(int start, int length) => new(RawArrayUnsafe, start, length);

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_rented is not null)
        {
            var rented = Interlocked.Exchange(ref _rented, null);
            if (disposing && rented is not null)
            {
                _pool.Return(rented);
            }
        }
    }

    [MemberNotNull(nameof(_rented))]
    private void ThrowIfDisposed()
    {
        if (_rented is null)
            throw new ObjectDisposedException(GetType().FullName);
    }
}