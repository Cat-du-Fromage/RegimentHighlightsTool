using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Mathf;

namespace Kaizerwald
{
    public static class UnityMathematicsExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this float lhs)
        {
            return Approximately(lhs, 0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this float2 lhs)
        {
            return Approximately(lhs.x, 0) && Approximately(lhs.y, 0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this float3 lhs)
        {
            return Approximately(lhs.x, 0) && Approximately(lhs.y, 0) && Approximately(lhs.z, 0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool approximately(this float2 lhs, float2 rhs)
        {
            return Approximately(lhs.x, rhs.x) && Approximately(lhs.y, rhs.y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool approximately(this float3 lhs, float3 rhs)
        {
            return Approximately(lhs.x, rhs.x) && Approximately(lhs.y, rhs.y) && Approximately(lhs.z, rhs.z);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Get Cross Vector for float2
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        // Equivalent to Vector2.Perpendicular Except default is clockwise
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Cross(this float2 value, bool clockwise = true)
        {
            return clockwise ? new float2(value.y, -value.x) : new float2(-value.y, value.x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 CrossRight(this float2 value) => value.Cross();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 CrossLeft(this float2 value) => value.Cross(false);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Sizzling float2
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 xay(this float2 value, float a) => new (value.x, a, value.y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 x0y(this float2 value) => value.xay(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 x1y(this float2 value) => value.xay(1);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Sizzling float3
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 xaz(this float3 value, float a) => new (value.x, a, value.z);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 x0z(this float3 value) => value.xaz(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 x1z(this float3 value) => value.xaz(1);
    }
}
