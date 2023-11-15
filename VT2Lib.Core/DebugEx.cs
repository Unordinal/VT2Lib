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
}