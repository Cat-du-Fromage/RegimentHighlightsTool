using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Mathf;

namespace KaizerWald
{
    public static class UnityMathematicsExtension
    {
        public static bool Approximately(this float2 lhs, float2 rhs)
        {
            return Mathf.Approximately(lhs.x, rhs.x) && Mathf.Approximately(lhs.y, rhs.y);
        }
        
        public static bool Approximately(this float3 lhs, float3 rhs)
        {
            return Mathf.Approximately(lhs.x, rhs.x) 
                   && Mathf.Approximately(lhs.y, rhs.y)
                   && Mathf.Approximately(lhs.z, rhs.z);
        }
        
        // Equivalent to Vector2.Perpendicular Except default is clockwise
        public static float2 Cross(this float2 value, bool clockwise = true)
        {
            return clockwise ? new float2(value.y, -value.x) : new float2(-value.y, value.x);
        }
        
        public static float2 CrossClockWise(this float2 value) => value.Cross();
        public static float2 CrossCounterClockWise(this float2 value) => value.Cross(false);
    }
}