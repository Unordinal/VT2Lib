using System.Numerics;
using System.Runtime.CompilerServices;
using Ai = Assimp;

namespace VT2Lib.Core.Extensions;

internal static class AssimpExtensions
{
    public static Ai.Matrix4x4 ToAssimp(this Matrix4x4 value)
    {
        return Unsafe.BitCast<Matrix4x4, Ai.Matrix4x4>(value);
    }
}