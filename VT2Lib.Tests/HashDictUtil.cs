using System.Globalization;
using System.Text.RegularExpressions;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;

namespace VT2Lib.Tests;

internal static partial class HashDictUtil
{
    private static readonly string TestHashDictionariesPath = Path.Combine(ProjectSource.ProjectDirectory, @"TestFiles\Hash Dictionaries\");
    public static readonly string HashSearchListFilePath = Path.Combine(TestHashDictionariesPath, @"vt2lib_hash_search_list.txt");


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
}