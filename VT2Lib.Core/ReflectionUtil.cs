using System.Diagnostics.CodeAnalysis;
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

    // https://github.com/dotnet/csharplang/issues/149#issuecomment-296172573
    [return: NotNullIfNotNull(nameof(@delegate))]
    internal static TDelegate? CastDelegate<TDelegate>(this Delegate @delegate)
        where TDelegate : Delegate
    {
        if (@delegate is null)
            return null;

        var multicastList = (@delegate as MulticastDelegate)?.GetInvocationList();
        if (multicastList is not null)
        {
            switch (multicastList.Length)
            {
                case 0:
                    return null!;
                case 1:
                    if (multicastList[0] != @delegate)
                        return multicastList[0].CastDelegate<TDelegate>();
                    break;
                default:
                    var converted = multicastList.Select(d => d.CastDelegate<TDelegate>());
                    return (TDelegate)Delegate.Combine(converted.Cast<Delegate>().ToArray())!;
            }
        }

        return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), @delegate.Target, @delegate.Method);
    }
}