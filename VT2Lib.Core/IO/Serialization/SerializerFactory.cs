using System.Diagnostics;
using VT2Lib.Core.Stingray.IO.Serialization;

namespace VT2Lib.Core.IO.Serialization;

public sealed class SerializerFactory
{
    public static SerializerFactory Default { get; }

    private readonly Dictionary<Type, Func<object>> _factories = new();

    static SerializerFactory()
    {
        Default = new();
        Default.RegisterSerializer(() => IDString32Serializer.Default);
        Default.RegisterSerializer(() => IDString64Serializer.Default);

        Default.RegisterSerializer(() => MeshGeometrySerializer.Default);
        Default.RegisterSerializer(() => SkinDataSerializer.Default);
        Default.RegisterSerializer(() => SimpleAnimationGroupSerializer.Default);
        Default.RegisterSerializer(() => SceneGraphSerializer.Default);
        Default.RegisterSerializer(() => MeshObjectSerializer.Default);
    }

    public void RegisterSerializer<T>(Func<ISerializer<T>> serializerFactory)
    {
        var type = typeof(T);
        if (_factories.ContainsKey(type))
            Trace.TraceWarning($"SerializerFactory already contains serializer of type {type.Name}. Replacing.");

        _factories[type] = serializerFactory;
    }

    public ISerializer<T> GetSerializer<T>()
    {
        return (ISerializer<T>)_factories[typeof(T)]();
    }
}