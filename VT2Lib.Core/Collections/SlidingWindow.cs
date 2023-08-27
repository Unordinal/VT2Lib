using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace VT2Lib.Core.Collections;

internal class SlidingWindow<T> : IEnumerable<SlidingWindow<T>.Window>
{
    /// <summary>
    /// Gets the size of the sliding window.
    /// </summary>
    public int WindowSize => _windowSize;

    private readonly IEnumerable<T> _collection;
    private readonly int _windowSize;

    public SlidingWindow(IEnumerable<T> collection, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (windowSize < 0)
            throw new ArgumentOutOfRangeException(nameof(windowSize));
        if (collection.TryGetNonEnumeratedCount(out int nonEnumCount))
        {
            if (nonEnumCount < windowSize)
                ThrowNotEnoughElementsException(nameof(collection), windowSize);
        }

        _collection = collection;
        _windowSize = windowSize;
    }

    public void TestingMethod()
    {
        SlidingWindow<char> slidingWindow = new(Array.Empty<char>(), 5);
        foreach (var window in slidingWindow)
        {
            foreach (var c in window)
            {

            }
        }
    }

    public IEnumerator<SlidingWindow<T>.Window> GetEnumerator()
    {
        T[] buffer = new T[_windowSize];
        int windowStart = 0;

        using var enumerator = _collection.GetEnumerator();
        int index = 0;
        do
        {
            if (!enumerator.MoveNext())
                ThrowNotEnoughElementsException("collection", _windowSize); // EVAL: change to a different exception?

            buffer[index] = enumerator.Current;
        }
        while (index < _windowSize);

        // We use a box to keep track of a buffer version. Each yield, we'll increment the version.
        // This ensures the yielded window can't be used once it's invalid.
        var version = new Box<int>();
        // We have at least one window's worth of data, yield it.
        yield return new Window(buffer, windowStart, version);
        version.Value++;

        while (enumerator.MoveNext())
        {
            int replaceIndex = windowStart++ % _windowSize;
            buffer[replaceIndex] = enumerator.Current;

            yield return new Window(buffer, windowStart, version);
            version.Value++;
        }

        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [DoesNotReturn]
    private static void ThrowNotEnoughElementsException(string? paramName, int windowSize)
    {
        throw new ArgumentOutOfRangeException(paramName, $"The given collection did not have enough elements to create a window of size {windowSize}.");
    }

    /// <summary>
    /// A view over a collection of elements.
    /// This should not be used if another window has been retrieved from the source <see cref="SlidingWindow{T}"/>.
    /// </summary>
    public readonly struct Window : IReadOnlyList<T>
    {
        /// <summary>
        /// Gets the number of items within this window.
        /// </summary>
        public int Count
        {
            get
            {
                ThrowIfInvalid();
                return _buffer.Length;
            }
        }

        private readonly T[]? _buffer;
        private readonly int _start;
        private readonly Box<int>? _bufferVersion;
        private readonly int _version;

        // Index is relative to this window.
        public T this[int index] => Get(index);

        /// <summary>
        /// Creates a view over a buffer, starting at the specified position.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="start"></param>
        internal Window(T[] buffer, int start, Box<int> version)
        {
            Debug.Assert(buffer is not null);
            Debug.Assert(version is not null);
            _buffer = buffer;
            _start = start;
            _bufferVersion = version;
            _version = version.Value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            ThrowIfInvalid();
            for (int i = 0; i < _buffer.Length; i++)
                yield return GetInternal(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Get(int index)
        {
            ThrowIfInvalid();
            // Technically this works fine but we want to treat this like a regular list externally.
            if (index < 0 || index >= _buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return GetInternal(index);
        }

        public T[] ToArray()
        {
            ThrowIfInvalid();
            T[] buffer = new T[_buffer.Length];
            // TODO: array copy properly.
            throw new NotImplementedException();
        }

        private T GetInternal(int index)
        {
            Debug.Assert(_buffer is not null);
            int mappedIndex = GetWrappedIndex(_start + index, _buffer.Length);
            return _buffer[mappedIndex];
        }

        [MemberNotNull(nameof(_buffer))]
        private void ThrowIfInvalid()
        {
            if (_buffer is null)
                throw new InvalidOperationException("Cannot use an uninitialized Window.");
            Debug.Assert(_bufferVersion is not null);
            if (_version != _bufferVersion.Value)
                throw new InvalidOperationException("Cannot access an out-of-date Window.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetWrappedIndex(int index, int count)
        {
            return ((index % count) + count) % count;
        }
    }

    internal sealed record class Box<TValue>
    {
        public TValue Value { get; set; } = default!;
    }
}