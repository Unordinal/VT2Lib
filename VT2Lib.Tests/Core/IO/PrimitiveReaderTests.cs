using System.Diagnostics;
using VT2Lib.Core.IO;
using Xunit.Abstractions;

namespace VT2Lib.Tests.Core.IO;

public class PrimitiveReaderTests
{
    private readonly ITestOutputHelper _output;

    public PrimitiveReaderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static byte[] GetBasicNumbers() => new byte[] { 5, 0, 0, 0, 2, 0, 17, 0, 0, 0, 0, 0, 0, 0 };

    [Fact]
    public void Read_SimpleNumbersSigned()
    {
        // Arrange
        using PrimitiveReader reader = new(new MemoryStream(GetBasicNumbers()));
        int expectedInt = 5;
        short expectedShort = 2;
        long expectedLong = 17;

        // Act
        int actualInt = reader.ReadInt32LE();
        short actualShort = reader.ReadInt16LE();
        long actualLong = reader.ReadInt64LE();

        // Assert
        Assert.Equal(expectedInt, actualInt);
        Assert.Equal(expectedShort, actualShort);
        Assert.Equal(expectedLong, actualLong);
    }

    [Fact]
    public void Read_SimpleNumbersSignedParity()
    {
        byte[] number = new byte[] { 5, 0, 0, 0, 0, 0, 0, 0 };
        using PrimitiveReader reader = new(new MemoryStream(number));
        int expectedInt = 5;
        short expectedShort = 5;
        long expectedLong = 5;

        int actualInt = reader.ReadInt32LE();
        reader.BaseStream.Position = 0;
        short actualShort = reader.ReadInt16LE();
        reader.BaseStream.Position = 0;
        long actualLong = reader.ReadInt64LE();

        Assert.Equal(expectedInt, actualInt);
        Assert.Equal(expectedShort, actualShort);
        Assert.Equal(expectedLong, actualLong);
    }

    [Fact]
    public void Read_ThrowsAfterDisposed()
    {
        using PrimitiveReader reader = new(new MemoryStream(GetBasicNumbers()));
        reader.Dispose();
        Assert.Throws<ObjectDisposedException>(() => reader.ReadUInt32LE());
        Assert.Throws<ObjectDisposedException>(() => reader.ReadInt16BE());
        Assert.Throws<ObjectDisposedException>(() => reader.BaseStream.Position);
    }

    //[Fact]
    private void Reader_Benchmark()
    {
        var numbers = GetBasicNumbers();
        Benchmark(() =>
        {
            using PrimitiveReader reader = new(new MemoryStream(numbers));
            reader.ReadUInt32LE();
        }, 100000, "reader read");
    }

    private void Benchmark(Action action, int iterations, string actionName)
    {
        action.Invoke();
        GC.Collect();
        _output.WriteLine("Starting benchmark for action: " + actionName);
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            action.Invoke();
        }
        sw.Stop();
        _output.WriteLine("Finished. Time elapsed: " + sw.Elapsed);
    }
}