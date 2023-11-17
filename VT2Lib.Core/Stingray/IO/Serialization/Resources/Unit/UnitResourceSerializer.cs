using VT2Lib.Core.Extensions;
using VT2Lib.Core.Stingray.Resources.Unit;

namespace VT2Lib.Core.Stingray.IO.Serialization.Resources.Unit;

public sealed class UnitResourceSerializer : ResourceSerializer<UnitResource>
{
    public static UnitResourceSerializer Default { get; }

    private readonly VersionedResourceSerializerProvider<UnitResource> _provider;

    static UnitResourceSerializer()
    {
        var versionedSerializersProvider = new VersionedResourceSerializerProvider<UnitResource>(UnitResource.ID);
        versionedSerializersProvider.RegisterSerializer(186, UnitResourceV186Serializer.Default);

        // TODO: This is temporary. 'units\architecture\broken_house\broken_house_roof_4m_01.unit' is version 189.
        versionedSerializersProvider.RegisterSerializer(189, UnitResourceV186Serializer.Default);

        Default = new(versionedSerializersProvider);
    }

    public UnitResourceSerializer(VersionedResourceSerializerProvider<UnitResource> versionedSerializerProvider)
        : base(UnitResource.ID)
    {
        _provider = versionedSerializerProvider;
    }

    public override void Serialize(Stream stream, UnitResource resource)
    {
        int version = resource.GetResourceVersion();
        var versionedSerializer = _provider.GetSerializer(version);
        versionedSerializer.Serialize(stream, resource);
    }

    public override UnitResource Deserialize(Stream stream)
    {
        int version = stream.ReadInt32LE();
        var versionedSerializer = _provider.GetSerializer(version);
        return versionedSerializer.Deserialize(stream);
    }
}