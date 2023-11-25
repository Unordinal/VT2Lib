namespace VT2Lib.Core.Stingray;

public sealed class VertexBuffer
{
    public required Validity Validity { get; set; }

    public required StreamType StreamType { get; set; }

    public required uint Count { get; set; }

    public required uint Stride { get; set; }

    public required Channel Channel { get; set; }

    public required byte[] Data { get; set; }

    public override string ToString()
    {
        return $"{Channel}[{Count}, {Stride}]";
    }
}