using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static UnityEngine.Mathf;

namespace KaizerWald
{
    public static class kzwGeometry
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PointInTriangle(this Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float s1 = c.y - a.y;
            float s2 = c.x - a.x;
            float s3 = b.y - a.y;
            float s4 = p.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - p.x * s1) / (s3 * s2 - (b.x-a.x) * s1);
            float w2 = (s4- w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PointInTriangle(this Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            float s1 = c.z - a.z;
            float s2 = c.x - a.x;
            float s3 = b.z - a.z;
            float s4 = p.z - a.z;

            float w1 = (a.x * s1 + s4 * s2 - p.x * s1) / (s3 * s2 - (b.x-a.x) * s1);
            float w2 = (s4- w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInTriangle(this float2 position2D, NativeArray<float2> triangle)
        {
            return position2D.IsPointInTriangle(triangle[0], triangle[1], triangle[2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInTriangle(this float2 position2D, NativeArray<float3> triangle)
        {
            return position2D.IsPointInTriangle(triangle[0].xz, triangle[1].xz, triangle[2].xz);
            /*
            float2 triA = triangle[0].xz;
            float2 triB = triangle[1].xz;
            float2 triC = triangle[2].xz;
            
            bool isAEqualC = Approximately(triC.y, triA.y);
            float2 a = isAEqualC ? triB : triA;
            float2 b = isAEqualC ? triA : triB;
            
            float s1 = triC.y - a.y;
            float s2 = triC.x - a.x;

            float s3 = b.y - a.y;
            float s4 = position2D.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - position2D.x * s1) / (s3 * s2 - (b.x - a.x) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
            */
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInTriangle(this float3 position, float2 triA, float2 triB, float2 triC)
        {
            return position.xz.IsPointInTriangle(triA, triB, triC);
            /*
            bool isAEqualC = Approximately(triC.y, triA.y);
            float2 a = isAEqualC ? triB : triA;
            float2 b = isAEqualC ? triA : triB;
            
            float s1 = triC.y - a.y;
            float s2 = triC.x - a.x;

            float s3 = b.y - a.y;
            float s4 = position.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - position.x * s1) / (s3 * s2 - (b.x - a.x) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
            */
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInTriangle(this float2 position, float2 triA, float2 triB, float2 triC)
        {
            bool isAEqualC = Approximately(triC.y, triA.y);
            float2 a = isAEqualC ? triB : triA;
            float2 b = isAEqualC ? triA : triB;
            
            float s1 = triC.y - a.y;
            float s2 = triC.x - a.x;

            float s3 = b.y - a.y;
            float s4 = position.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - position.x * s1) / (s3 * s2 - (b.x - a.x) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInTriangle(this float3 position, float3 triA, float3 triB, float3 triC)
        {
            return position.IsPointInTriangle(triA.xz, triB.xz, triC.xz);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInTriangle(this Vector3 position, Vector3 triA, Vector3 triB, Vector3 triC)
        {
            return ((float3)position).IsPointInTriangle(triA, triB, triC);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PointInTriangle2(this Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 d = b - a;
            Vector3 e = c - a;
            e.y = Approximately(e.y, 0) ? 0.0001f : e.y;//safety
            float w1 = (e.x * (a.y - p.y) + e.y * (p.x - a.x)) / (d.x * e.y - d.y * e.x);
            float w2 = (p.y - a.y - w1 * d.y) / e.y;
            return (w1 >= 0f) && (w2 >= 0f) && ((w1 + w2) <= 1f);
        } 
    }
}
