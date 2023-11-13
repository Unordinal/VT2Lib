using System.Reflection;

namespace VT2Lib.Core;

internal static class ReflectionUtil
{
    internal static IEnumerable<Type> GetAllSubclasses<T>()
    {
        return GetAllSubclasses(typeof(T));
    }

    internal static IEnumerable<Type> GetAllSubclasses(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var allSubclasses = AppDomain.CurrentDomain.GetAssemblies().AsParallel()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(type));

        return allSubclasses;
    }

    internal static IEnumerable<(Type, TAttribute)> GetAllWithAttribute<TAttribute>(bool inherit)
        where TAttribute : Attribute
    {
        var allTypesWithAttr = AppDomain.CurrentDomain.GetAssemblies().AsParallel()
            .SelectMany(t => t.GetTypes())
            .Where(t => t.IsDefined(typeof(TAttribute), inherit))
            .Select(t => (Type: t, Attribute: t.GetCustomAttribute<TAttribute>(inherit)!));

        return allTypesWithAttr;
    }
}