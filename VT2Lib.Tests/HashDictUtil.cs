using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Hashing;

namespace VT2Lib.Tests;

internal static partial class HashDictUtil
{
    private static readonly string TestHashDictionariesPath = Path.Combine(ProjectSource.ProjectDirectory, @"TestFiles\Hash Dictionaries\");
    public static readonly string HashSearchListFilePath = Path.Combine(TestHashDictionariesPath, @"vt2lib_hash_search_list.txt");
    public static readonly string ModelHashSearchDictFilePath = Path.Combine(TestHashDictionariesPath, @"vt2lib_model_hash_search_dict.json");


    private static bool _prepared = false;

    public static void PrepareKnownHashes()
    {
        if (_prepared)
            return;

        _prepared = true;
        foreach (var dict in GetHashDictFiles())
        {
            foreach (var line in File.ReadLines(dict))
            {
                var split = line.Split((char[]?)null, 2, StringSplitOptions.TrimEntries);
                if (split.Length == 1)
                    continue;

                int hashSize = split[0].Length == 16 ? 64 : 32;
                if (hashSize == 64)
                {
                    ulong hash = ulong.Parse(line[..16], NumberStyles.HexNumber);
                    string hashValue = split[1];

                    IDStringRepository.Shared.TryAdd(new IDString64(hash, hashValue));
                }
                else
                {
                    uint hash = uint.Parse(line[..8], NumberStyles.HexNumber);
                    string hashValue = split[1];

                    IDStringRepository.Shared.TryAdd(new IDString32(hash, hashValue));
                }
            }
        }
    }

    public static HashedSearchDict GetSearchDict()
    {
        return new HashedSearchDict(ModelHashSearchDictFilePath);
    }

    private static IEnumerable<string> GetHashDictFiles()
    {
        foreach (var file in Directory.EnumerateFiles(TestHashDictionariesPath))
        {
            if (!HashDictNameRegex().IsMatch(file))
                continue;

            yield return file;
        }
    }

    [GeneratedRegex(@"vt2lib_hashdict(?<hashSize>32|64)?.*\.txt$")]
    private static partial Regex HashDictNameRegex();

    internal class HashedSearchDict
    {
        private readonly Dictionary<uint, IDString32> _hashedDict;

        internal HashedSearchDict(string jsonFilePath)
        {
            using var file = File.OpenRead(jsonFilePath);
            var json = JsonSerializer.Deserialize<JsonObject>(file);
            var prefixesArr = json!["prefixes"]!.AsArray();
            var suffixesArr = json!["suffixes"]!.AsArray();
            var baseArr = json!["base"]!.AsArray();
            prefixesArr.Add(string.Empty);
            suffixesArr.Add(string.Empty);

            var combined = from prefixVal in prefixesArr
                           from baseVal in baseArr
                           from suffixVal in suffixesArr
                           select $"{prefixVal}{baseVal}{suffixVal}";

            _hashedDict = combined
                .Select(v => new IDString32(v))
                .ToDictionary(ids => ids.ID);
        }

        public bool TryFindHash(uint hash, out IDString32 idString)
        {
            return _hashedDict.TryGetValue(hash, out idString);
        }
    }
}