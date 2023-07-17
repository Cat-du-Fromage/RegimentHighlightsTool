using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public static class UnityMathematicsUtilities
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// ComponentWise Multiplication (x * y)
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int cmul(int2 v) => v.x * v.y;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float cmul(float2 v) => v.x * v.y;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// ComponentWise subtraction (x - y)
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int csub(int2 v) => v.x - v.y;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Component-Z Dot product (x - y)
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CzDot(float2 a, float2 b)
        {
            float cz = a.x * b.y - a.y * b.x;
            return cz;
        }
    }
}
