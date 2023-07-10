using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public static class KzwMath
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ Grid Helpers ◆◆◆◆◆◆                                              ║
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
            int x = index - (y * width);
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
            (int x, int y) = GetXY(index, width);
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
            int y = (index % (width * width)) / width;
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

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Square value : Multiply value by itself (v * v)
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Square(int v) => v * v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Square(float v) => v * v;
    }
}
