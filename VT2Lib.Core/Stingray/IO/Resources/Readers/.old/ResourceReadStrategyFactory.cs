using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VT2Lib.Core.Stingray.IO.Resources.Readers.Strategies;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

internal sealed class ResourceReadStrategyFactory
{
    public static ResourceReadStrategyFactory SharedBinaryReadStrategies { get; } = new();

    private readonly ConcurrentDictionary<IDString64, ConcurrentDictionary<int, Func<IResourceReadStrategy>>> _factory = new();

    static ResourceReadStrategyFactory()
    {
        SharedBinaryReadStrategies = new ResourceReadStrategyFactory();
    }

    public bool TryRegister(IDString64 resourceType, int resourceVersion, Func<IResourceReadStrategy> strategyCtor)
    {
        if (_factory.TryGetValue(resourceType, out var versionFactory))
            return versionFactory.TryAdd(resourceVersion, strategyCtor);

        versionFactory = new ConcurrentDictionary<int, Func<IResourceReadStrategy>>
        {
            [resourceVersion] = strategyCtor
        };

        return _factory.TryAdd(resourceType, versionFactory);
    }

    public bool TryUnregister(IDString64 resourceType, int resourceVersion)
    {
        if (!_factory.TryGetValue(resourceType, out var versionFactory))
            return false;

        return versionFactory.TryRemove(resourceVersion, out _);
    }

    public IResourceReadStrategy GetStrategy(IDString64 resourceType, int resourceVersion)
    {
        if (!TryGetStrategy(resourceType, resourceVersion, out var strategy))
            throw new ArgumentException($"No strategy registered for type '{resourceType}', version '{resourceVersion}'");

        return strategy;
    }

    public bool TryGetStrategy(IDString64 resourceType, int resourceVersion, [NotNullWhen(true)] out IResourceReadStrategy? strategy)
    {
        strategy = default;
        if (!_factory.TryGetValue(resourceType, out var versionFactory))
            return false;

        bool success = versionFactory.TryGetValue(resourceVersion, out var strategyCtor);
        if (success)
            strategy = strategyCtor!();

        return versionFactory.TryGetValue(resourceVersion, out strategyCtor);
    }
}