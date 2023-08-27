using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2Lib.Core.Collections;

// Based off of .NET ValueStringBuilder.
internal ref struct ValueList<T>
    where T : unmanaged
{
    private T[]? _rentedArray;
    private Span<T> _buffer;
    private int _pos;

    public int Length => _pos;

    public int Capacity => _buffer.Length;

    public Span<T> RawBuffer => _buffer;

    public ref T this[int index] => ref _buffer[index];

    public ValueList(Span<T> initialBuffer)
    {
        _rentedArray = null;
        _buffer = initialBuffer;
        _pos = 0;
    }

    public ValueList(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));

        _rentedArray = ArrayPool<T>.Shared.Rent(initialCapacity);
        _buffer = _rentedArray;
        _pos = 0;
    }

    /// <summary>
    /// Creates an array from the contents of this <see cref="ValueList{T}"/> and disposes this instance.
    /// </summary>
    /// <returns></returns>
    public T[] ToArray()
    {
        T[] result = _buffer[.._pos].ToArray();
        Dispose();
        return result;
    }

    public ReadOnlySpan<T> AsSpan() => _buffer[.._pos];

    public ReadOnlySpan<T> AsSpan(int start) => _buffer[start.._pos];

    public ReadOnlySpan<T> AsSpan(int start, int length) => _buffer.Slice(start, length);

    /// <summary>
    /// Copies the contents of this <see cref="ValueList{T}"/> into the given span and disposes of this instance.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="valuesWritten"></param>
    /// <returns></returns>
    public bool TryCopyTo(Span<T> destination, out int valuesWritten)
    {
        if (_buffer[.._pos].TryCopyTo(destination))
        {
            valuesWritten = _pos;
            Dispose();
            return true;
        }
        else
        {
            valuesWritten = 0;
            Dispose();
            return false;
        }
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        if ((uint)capacity > (uint)_buffer.Length)
            Grow(capacity - _pos);
    }

    public ref T GetPinnableReference()
    {
        return ref MemoryMarshal.GetReference(_buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value)
    {
        int pos = _pos;
        Span<T> buffer = _buffer;
        if ((uint)pos < (uint)buffer.Length)
        {
            buffer[pos] = value;
            _pos = pos + 1;
        }
        else
        {
            GrowAndAdd(value);
        }
    }

    public void AddRange(ReadOnlySpan<T> values)
    {
        if (values.IsEmpty)
            return;

        int pos = _pos;
        if (values.Length == 0 && (uint)pos < (uint)_buffer.Length)
        {
            _buffer[pos] = values[0];
            _pos = pos + 1;
        }
        else
        {
            AddRangeSlow(values);
        }
    }

    private void AddRangeSlow(ReadOnlySpan<T> values)
    {
        int pos = _pos;
        if (pos > _buffer.Length - values.Length)
        {
            Grow(values.Length);
        }

        values.CopyTo(_buffer[pos..]);
        _pos += values.Length;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAdd(T value)
    {
        Grow(1);
        Add(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(_pos > _buffer.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        const uint ArrayMaxLength = 0x7FFFFFC7;
        int newCapacity = (int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), Math.Min((uint)_buffer.Length * Unsafe.SizeOf<T>(), ArrayMaxLength));

        T[] rentedArray = ArrayPool<T>.Shared.Rent(newCapacity);
        _buffer[.._pos].CopyTo(rentedArray);

        T[]? toReturn = _rentedArray;
        _buffer = _rentedArray = toReturn;

        if (toReturn != null)
            ArrayPool<T>.Shared.Return(toReturn);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        T[]? toReturn = _rentedArray;
        this = default;
        if (toReturn is not null)
            ArrayPool<T>.Shared.Return(toReturn);
    }
}