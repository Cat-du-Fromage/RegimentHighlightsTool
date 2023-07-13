using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Mathf;
using static UnityEngine.Vector2;

using static Unity.Mathematics.math;
using static Unity.Mathematics.int2;
using static Unity.Mathematics.int3;
using static Unity.Mathematics.float2;
using static Unity.Mathematics.float3;
using static Unity.Mathematics.quaternion;

namespace KaizerWald
{
    public static class KzwMath
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ Grid Helpers ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Get position (in Int, Int) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="width">width(X) of the Grid</param>
        /// <returns>Int X, Int Y(return in this order)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int, int) GetXY(int index, int width)
        {
            int y = index / width;
            int x = index - y * width;
            return (x, y);
        }

        /// <summary>
        /// Get position (in Int2) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="width">width(X) of the grid</param>
        /// <returns>Int2 Pos</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetXY2(int index, int width)
        {
            int y = index / width;
            int x = index - y * width;
            return new int2(x, y);
        }

        /// <summary>
        /// USE FOR VOXEL GENERATION TYPE 3D
        /// </summary>
        /// <param name="index">index in the grid</param>
        /// <param name="width">width(X) of the grid</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int, int, int) GetXYZ(int index, int width)
        {
            int x = index % width;
            int y = index % (width * width) / width;
            int z = index / (width * width);
            return (x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 GetXYZ3(int i, int width)
        {
            (int x, int y, int z) = GetXYZ(i, width);
            return new int3(x, y, z);
        }

        /// <summary>
        /// Get array Index from Coord
        /// <param name="coord">coord in the grid</param>
        /// <param name="width">width(X) of the grid</param>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndex(in int2 coord, int width)
        {
            return coord.y * width + coord.x;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndex(int x, int y, int width)
        {
            return y * width + x;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Square value : Multiply value by itself (v * v)
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Square(int v) => v * v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Square(float v) => v * v;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Determinant of 2 Vectors
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Determinant(float v1x, float v1y, float v2x, float v2y)
        {
            return determinant(new float2x2(v1x, v1y,v2x, v2y)); //(v1x * v2y) - (v1y * v2x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Determinant(in float2 v1, in float2 v2)
        {
            return determinant(new float2x2(v1, v2)); //Det(v1.x, v1.y, v2.x, v2.y);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Get Intersection Between 2 Vectors(and their respective direction)
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetIntersection(Vector2 originX, Vector2 originY, Vector2 dirX, Vector2 dirY)
        {
            Vector2 thisPoint2 = originX + dirX;
            Vector2 otherPoint2 = originY + dirY;

            float a1 = thisPoint2.y - originX.y;
            float b1 = originX.x - thisPoint2.x;
            float c1 = a1 * originX.x + b1 * originX.y;

            float a2 = otherPoint2.y - originY.y;
            float b2 = originY.x - otherPoint2.x;
            float c2 = a2 * originY.x + b2 * originY.y;

            float det = a1 * b2 - a2 * b1;
            bool isDetZero = Approximately(det, 0);
            return isDetZero ? negativeInfinity : new Vector2((b2 * c1 - b1 * c2) / det, (a1 * c2 - a2 * c1) / det);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetIntersection(float2 originX, float2 originY, float2 dirX, float2 dirY)
        {
            float2 thisPoint2 = originX + dirX;
            float2 otherPoint2 = originY + dirY;
            //This line calculates the change in y-coordinates between the two points on the first line.
            float a1 = thisPoint2.y - originX.y;
            //This line calculates the change in x-coordinates between the two points on the first line, but with the sign reversed.
            float b1 = originX.x - thisPoint2.x;
            //This line calculates a constant value for the first line using its two points.
            float c1 = a1 * originX.x + b1 * originX.y;

            //This line calculates the change in y-coordinates between the two points on the second line.
            float a2 = otherPoint2.y - originY.y;
            //This line calculates the change in x-coordinates between the two points on the second line, but with the sign reversed.
            float b2 = originY.x - otherPoint2.x;
            //This line calculates a constant value for the second line using its two points.
            float c2 = a2 * originY.x + b2 * originY.y;

            //This line calculates the determinant of a matrix formed by coefficients of both lines.
            float det = a1 * b2 - a2 * b1;
            //This line checks if the determinant is approximately zero, which means that the lines are parallel or coincident.
            bool isDetZero = Approximately(det, 0);
            return isDetZero ? NegativeInfinity : float2((b2 * c1 - b1 * c2) / det, (a1 * c2 - a2 * c1) / det);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetIntersection3DFlat(float3 originX, float3 originY, float3 dirX, float3 dirY)
        {
            float2 intersectPoint = GetIntersection(originX.xz, originY.xz, dirX.xz, dirY.xz);
            return new float3(intersectPoint.x, 0, intersectPoint.y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetIntersection3DFlat(Vector3 originX, Vector3 originY, Vector3 dirX, Vector3 dirY)
        {
            return GetIntersection3DFlat((float3)originX, (float3)originY, (float3)dirX, (float3)dirY);
        }
    }
}
