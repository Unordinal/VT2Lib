using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System.Buffers;
using System.Globalization;
using VT2Lib.Core.Stingray;
using VT2Lib.Core.Stingray.Collections;
using VT2Lib.Core.Stingray.Hashing;

namespace VT2Lib.Benchmarking;

[Config(typeof(AntiVirusFriendlyConfig))]
public class HashDicts
{
    public (string, string)[] HashValuePairs { get; set; }

    private bool _result;

    private Consumer _consumer;

    public HashDicts()
    {
    }

    [GlobalSetup]
    public void Setup()
    {
        _consumer = new Consumer();
        HashValuePairs = HashDictUtil.GetHashDictFiles(64)
            .SelectMany(HashDictUtil.GetHashes)
            .ToArray();
    }

    [Benchmark]
    public void BenchHashAllValues() => HashAllValues().Consume(_consumer);

    [Benchmark]
    public void BenchHashAllValuesRaw() => HashAllValuesRaw().Consume(_consumer);

    [Benchmark]
    public void BenchParseAllValues() => ParseAllValues().Consume(_consumer);

    [Benchmark]
    public void BenchReadAllValuesToRepo()
    {
        var hvs = HashDictUtil.GetHashDictFiles(64)
            .SelectMany(HashDictUtil.GetHashes);

        foreach (var (hash, hashValue) in hvs)
        {
            var hashNum = ulong.Parse(hash, NumberStyles.HexNumber);
            if (IDStringRepository.Shared.Contains(hashNum))
                continue;

            IDStringRepository.Shared.Add(new IDString64(hashNum, hashValue));
            _result = IDStringRepository.Shared.Get(hashNum) != default;
        }
    }

    [Benchmark]
    public void BenchReadAllValuesToRepoTry()
    {
        var hvs = HashDictUtil.GetHashDictFiles(64)
            .SelectMany(HashDictUtil.GetHashes);

        foreach (var (hash, hashValue) in hvs)
            _result = IDStringRepository.Shared.TryAdd(new IDString64(ulong.Parse(hash, NumberStyles.HexNumber), hashValue));

        _result = !_result;
    }

    [Benchmark]
    public void BenchReadAndHashAllValuesToRepoTry()
    {
        var hvs = HashDictUtil.GetHashDictFiles(64)
            .SelectMany(HashDictUtil.GetHashes);

        foreach (var (_, hashValue) in hvs)
            _result = IDStringRepository.Shared.TryAdd(new IDString64(hashValue));

        _result = !_result;
    }

    public IEnumerable<IDString64> HashAllValues()
    {
        foreach (var (hash, value) in HashValuePairs)
            yield return new IDString64(value);
    }

    public IEnumerable<ulong> HashAllValuesRaw()
    {
        foreach (var (hash, value) in HashValuePairs)
            yield return Murmur.Hash64(value);
    }

    public IEnumerable<(ulong, string)> ParseAllValues()
    {
        foreach (var (hash, value) in HashValuePairs)
            yield return (ulong.Parse(hash, NumberStyles.HexNumber), value);
    }
}