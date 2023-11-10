using System.Globalization;
using System.Text.RegularExpressions;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;

namespace VT2Lib.Tests;

internal static partial class HashDictUtil
{
    private static readonly string TestHashDictionariesPath = Path.Combine(ProjectSource.ProjectDirectory, @"TestFiles\Hash Dictionaries\");

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
                if (line.Length < 16)
                    continue;

                ulong hash = ulong.Parse(line[..16], NumberStyles.HexNumber);
                string hashValue = line[18..];
                IDStringRepository.Shared.TryAdd(new IDString64(hash, hashValue));
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

    [GeneratedRegex(@"vt2bu_.*\.txt$")]
    private static partial Regex HashDictNameRegex();
}