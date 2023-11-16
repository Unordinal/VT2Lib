using VT2Lib.Core.Stingray.IO.Serialization.Resources;
using VT2Lib.Core.Stingray.Resources.Unit;
using Xunit.Abstractions;

namespace VT2Lib.Tests.Core.Stingray.Resources;

public class UnitResourceTests
{
    private const string TestUnitResourceFile = @".extracted_resources_test\units\beings\critters\chr_critter_crow\chr_critter_crow.unit";
    private readonly ITestOutputHelper _output;

    public UnitResourceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(GetTestUnitResourceFiles))]
    public void ReadUnitFiles(string unitFile)
    {
        using var unitStream = File.OpenRead(unitFile);
        var serializer = ResourceSerializerProvider.Default.GetSerializer(UnitResource.ID);

        var resource = serializer.DeserializeAs<UnitResource>(unitStream);
        _output.WriteLine(resource.ToString());
    }

    public static IEnumerable<object[]> GetTestUnitResourceFiles()
    {
        yield return new object[] { TestUnitResourceFile };
    }
}