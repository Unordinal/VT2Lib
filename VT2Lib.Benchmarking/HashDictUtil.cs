using System.Globalization;
using System.Text.RegularExpressions;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;

namespace VT2Lib.Benchmarking;

internal static partial class HashDictUtil
{
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

    public static IEnumerable<(string Hash, string Value)> GetHashes(string dictPath)
    {
        foreach (var line in File.ReadLines(dictPath))
        {
            var split = line.Split((char[]?)null, 2, StringSplitOptions.TrimEntries);
            if (split.Length == 1)
                continue;

            if (split[0].Length == 16)
            {
                yield return (line[..16], split[1]);
            }
            else if (split[0].Length == 8)
            {
                yield return (line[..8], split[1]);
            }
        }
    }

    public static IEnumerable<string> GetHashDictFiles(int? hashSize = null)
    {
        foreach (var file in Directory.EnumerateFiles(TestPaths.HashDictionariesPath))
        {
            var dictFileMatch = HashDictNameRegex().Match(file);
            if (dictFileMatch is null)
                continue;

            if (hashSize.HasValue && dictFileMatch.Groups["hashSize"].Value != hashSize.Value.ToString())
                continue;

            yield return file;
        }
    }

    [GeneratedRegex(@"vt2lib_hashdict(?<hashSize>32|64)?_.*\.txt$")]
    private static partial Regex HashDictNameRegex();
}