using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2Lib.Core.Collections;

namespace VT2Lib.Bundles.IO;
internal class ProtoChunkDecompressorStream
{
    public int MaxChunkLength => _maxChunkLength;

    private readonly Stream _stream;
    private readonly int _maxChunkLength;
    private readonly int _numChunksToBuffer;
    private readonly bool _leaveOpen;
    private bool _disposed;

    private readonly RentedArray<byte> _buffer;

    public ProtoChunkDecompressorStream(Stream stream, int maxChunkLength, int numChunksToBuffer, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
            throw new ArgumentException("The stream does not support seeking.", nameof(stream));
        if (numChunksToBuffer is <= 0 and not -1)
            throw new ArgumentOutOfRangeException(nameof(numChunksToBuffer), "The number of chunks to buffer must be a positive number or -1.");

        _stream = stream;
        _maxChunkLength = maxChunkLength;
        _numChunksToBuffer = numChunksToBuffer;
        _leaveOpen = leaveOpen;

        _buffer = new RentedArray<byte>(maxChunkLength * numChunksToBuffer);
    }
}
