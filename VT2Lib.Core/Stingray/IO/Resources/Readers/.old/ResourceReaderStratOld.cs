using VT2Lib.Core.Extensions;
using VT2Lib.Core.Stingray.Resources;

namespace VT2Lib.Core.Stingray.IO.Resources.Readers;

internal class ResourceReaderStratOld : IResourceReader
{
    private readonly IDString64 _resourceType;
    private readonly ResourceReadStrategyFactory _readStrategyFactory;

    public ResourceReaderStratOld(IDString64 resourceType, ResourceReadStrategyFactory readStrategyFactory)
    {
        _resourceType = resourceType;
        _readStrategyFactory = readStrategyFactory;
    }

    public TResource Read<TResource>(Stream stream) where TResource : IResource
    {
        return (TResource)Read(stream);
    }

    public IResource Read(Stream stream)
    {
        int resourceVersion = stream.ReadInt32();
        var readStrategy = _readStrategyFactory.GetStrategy(_resourceType, resourceVersion);
        return readStrategy.Read(stream);
    }

    public static ResourceReaderStratOld CreateBinaryReader(IDString64 resourceType)
    {
        return new ResourceReaderStratOld(resourceType, ResourceReadStrategyFactory.SharedBinaryReadStrategies);
    }
}