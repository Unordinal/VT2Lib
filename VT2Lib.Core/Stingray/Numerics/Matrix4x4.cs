using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VT2Lib.Core.Stingray.Numerics;

// Use System.Numerics.Matrix4x4 instead?
[StructLayout(LayoutKind.Sequential)]
public struct Matrix4x4
{
    private const float DecomposeEpsilon = 0.0001f;

    public static Matrix4x4 Identity => new
    (
        1f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f,
        0f, 0f, 1f, 0f,
        0f, 0f, 0f, 1f
    );

    public float M11, M12, M13, M14,
                 M21, M22, M23, M24,
                 M31, M32, M33, M34,
                 M41, M42, M43, M44;

    public readonly bool IsIdentity
    {
        get
        {
            return M11 == 1f && M22 == 1f && M33 == 1f && M44 == 1f && // Check diagonal element first for early out.
                                M12 == 0f && M13 == 0f && M14 == 0f &&
                   M21 == 0f && M23 == 0f && M24 == 0f &&
                   M31 == 0f && M32 == 0f && M34 == 0f &&
                   M41 == 0f && M42 == 0f && M43 == 0f;
        }
    }

    public Vector3 Translation
    {
        readonly get => new(M41, M42, M43);
        set
        {
            M41 = value.X;
            M42 = value.Y;
            M43 = value.Z;
        }
    }

    public Matrix4x4(
        float m11, float m12, float m13, float m14, 
        float m21, float m22, float m23, float m24, 
        float m31, float m32, float m33, float m34, 
        float m41, float m42, float m43, float m44)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M14 = m14;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M24 = m24;
        M31 = m31;
        M32 = m32;
        M33 = m33;
        M34 = m34;
        M41 = m41;
        M42 = m42;
        M43 = m43;
        M44 = m44;
    }
}
