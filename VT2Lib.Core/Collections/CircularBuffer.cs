using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VT2Lib.Core.Collections;
internal class CircularBuffer<T>
{
    public int Count => _count;

    public int Capacity => _buffer.Length;

    private readonly T[] _buffer;
    private int _start;
    private int _end;
    private int _count;

    public CircularBuffer(int capacity)
    {
        _buffer = new T[capacity];
    }

    public CircularBuffer(T[] buffer, int start, int count)
    {
        _buffer = buffer;
        _start = start;
        _end = count - start;
        _count = count;
    }
}
