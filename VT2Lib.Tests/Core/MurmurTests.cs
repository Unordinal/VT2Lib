using VT2Lib.Core.Stingray.Hashing;
using Xunit.Abstractions;

namespace VT2Lib.Tests.Core;

public class MurmurTests
{
    private const string LongformHash1 = @"scripts/game/entity_system/systems/ai/generated/behaviour_trees/bt_goblin_sprayer_fire_use_abilities_jump_grab";
    private const string LongformHash2 = @"scripts/game/entity_system/systems/ai/generated/behaviour_trees/bt_dwarf_highpriest_00_use_abilities_jump_grab";
    private const string LongformHash3 = @"scripts/game/entity_system/systems/ai/generated/behaviour_trees/bt_dwarf_lighpriest_00_use_abilities_jump_grab";
    private readonly ITestOutputHelper _output;

    public MurmurTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(new byte[] { }, 0x0ul)]
    [InlineData(new byte[] { 1, 2, 3, 4 }, 0xF85CFF3275DF7618ul)]
    [InlineData(new byte[] { 114, 111, 111, 116, 95, 112, 111, 105, 110, 116 }, 0xDE542DA9CF3A5A5Eul)] // root_point
    public void Murmur_Hash64Span(byte[] key, ulong expectedHash)
    {
        ulong actualHash = Murmur.Hash64(key);
        Assert.Equal(expectedHash, actualHash);
    }

    [Theory]
    [InlineData("", 0x0ul)]
    [InlineData("test", 0x2F4A8724618F4C63ul)]
    [InlineData("root_point", 0xDE542DA9CF3A5A5Eul)]
    [InlineData("particles", 0xA8193123526FAD64ul)]
    [InlineData("lua", 0xA14E8DFA2CD117E2ul)]
    [InlineData("shading_environment_mapping", 0x250E0A11AC8E26F8ul)]
    [InlineData(LongformHash1, 0x039989A77A491F70ul)]
    [InlineData(LongformHash2, 0x03F01DBDC4AF2705ul)]
    [InlineData(LongformHash3, 0xC88CE1481CA707D8ul)]
    public void Murmur_Hash64String(string key, ulong expectedHash)
    {
        ulong actualHash = Murmur.Hash64(key);
        Assert.Equal(expectedHash, actualHash);
    }

    [Theory]
    [InlineData("foo", 0x1234ABCD, 0x9DF7E7EE73C2B95Ful)]
    public void Murmur_Hash64StringWithSeed(string key, ulong seed, ulong expectedHash)
    {
        ulong actualHash = Murmur.Hash64(key, seed);
        Assert.Equal(expectedHash, actualHash);
    }

    [Theory]
    [InlineData("", 0x0)]
    [InlineData("root_point", 0xDE542DA9u)]
    [InlineData("particles", 0xA8193123u)]
    public void Murmur_Hash32String(string key, uint expectedHash)
    {
        uint actualHash = Murmur.Hash32(key);
        Assert.Equal(expectedHash, actualHash);
    }
}