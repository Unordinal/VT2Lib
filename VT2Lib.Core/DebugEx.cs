using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace VT2Lib.Core;

internal static class DebugEx
{
    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition, [CallerArgumentExpression(nameof(condition))] string? conditionExpr = null)
    {
        Debug.Assert(condition, conditionExpr);
    }

    public static string ListNulls(
        object? arg1,
        object? arg2 = null,
        object? arg3 = null,
        object? arg4 = null,
        object? arg5 = null,
        [CallerArgumentExpression(nameof(arg1))] string? arg1Name = null,
        [CallerArgumentExpression(nameof(arg2))] string? arg2Name = null,
        [CallerArgumentExpression(nameof(arg3))] string? arg3Name = null,
        [CallerArgumentExpression(nameof(arg4))] string? arg4Name = null,
        [CallerArgumentExpression(nameof(arg5))] string? arg5Name = null
        )
    {
        List<string> nullsList = new(5);
        if (arg1 is null && arg1Name is not null)
            nullsList.Add(arg1Name);
        if (arg2 is null && arg2Name is not null)
            nullsList.Add(arg2Name);
        if (arg3 is null && arg3Name is not null)
            nullsList.Add(arg3Name);
        if (arg4 is null && arg4Name is not null)
            nullsList.Add(arg4Name);
        if (arg5 is null && arg5Name is not null)
            nullsList.Add(arg5Name);

        return string.Join(", ", nullsList);
    }
}