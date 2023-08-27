using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VT2Lib.Core;

/// <summary>
/// Provides an easy way to allocate an array on the stack or rent one out if the length is too prohibitive to stackalloc.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
///     byte[] someArray = ...;
///     int length = someArray.Length;
///     const int MaxLength = 256;
///     using <see cref="StackAllocHelper{T}"/> buffer = length &lt;= MaxLength
///                                        ? new(<see langword="stackalloc"/> byte[length])
///                                        : new(length);
/// </code>
/// </remarks>
/// <typeparam name="T"></typeparam>
[SkipLocalsInit]
internal readonly ref struct StackAllocHelper<T>
{
    /// <summary>
    /// Gets a span over the array.
    /// </summary>
    public Span<T> Span => _span;

    private readonly Span<T> _span;
    private readonly ArrayPool<T>? _sourcePool;
    private readonly T[]? _rented;

    public StackAllocHelper(Span<T> buffer)
    {
        _span = buffer;
        _sourcePool = null;
        Unsafe.SkipInit(out _rented);
    }

    public StackAllocHelper(int length) : this(ArrayPool<T>.Shared, length)
    {
    }

    public StackAllocHelper(ArrayPool<T> sourcePool, int length)
    {
        ArgumentNullException.ThrowIfNull(sourcePool);
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        _sourcePool = sourcePool;
        _rented = sourcePool.Rent(length);
        _span = _rented.AsSpan(0, length);
    }

    /// <summary>
    /// Returns the rented array, if there was one.
    /// </summary>
    public void Dispose()
    {
        if (_sourcePool is not null)
        {
            Debug.Assert(_rented is not null);
            _sourcePool.Return(_rented);
        }
    }
}
