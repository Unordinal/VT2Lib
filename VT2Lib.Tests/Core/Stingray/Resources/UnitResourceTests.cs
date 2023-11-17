using System.Diagnostics;
using VT2Lib.Core.Stingray.IO.Serialization.Resources;
using VT2Lib.Core.Stingray.Resources.Unit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace VT2Lib.Tests.Core.Stingray.Resources;

public class UnitResourceTests
{
    private readonly ITestOutputHelper _output;

    public UnitResourceTests(ITestOutputHelper output)
    {
        _output = output;
        Trace.Listeners.Add(new TraceTestListener(_output));
    }

    [Theory]
    [MemberData(nameof(GetTestUnitResourceFiles))]
    public void ReadUnitFiles(string unitFile)
    {
        _output.WriteLine(Path.GetFileName(unitFile));
        using var unitStream = File.OpenRead(unitFile);
        var serializer = ResourceSerializerProvider.Default.GetSerializer(UnitResource.ID);

        var resource = serializer.DeserializeAs<UnitResource>(unitStream);
        _output.WriteLine(resource.ToString());
    }

    public static IEnumerable<object[]> GetTestUnitResourceFiles()
    {
        yield return new object[] { @".extracted_resources_test\units\beings\critters\chr_critter_crow\chr_critter_crow.unit" };
        yield return new object[] { @".extracted_resources_test\units\architecture\broken_house\broken_house_floor_4m_01.unit" };
        yield return new object[] { @".extracted_resources_test\units\architecture\broken_house\broken_house_roof_4m_01.unit" }; // unit version 189?!
        yield return new object[] { @".extracted_resources_test\units\vegetation\town\town_veg_hedge_solid_low_01.unit" };
        yield return new object[] { @".extracted_resources_test\units\vegetation\town\town_veg_tree_dead_01.unit" };
    }

    public class TraceTestListener : TraceListener
    {
        private readonly ITestOutputHelper _output;

        public TraceTestListener(ITestOutputHelper output)
        {
            _output = output;
        }

        public override void Write(string? message)
        {
            _output.Write(message ?? string.Empty);
        }

        public override void WriteLine(string? message)
        {
            _output.WriteLine(message ?? string.Empty);
        }
    }
}