using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using int2 = Unity.Mathematics.int2;

using Kaizerwald;
using static Kaizerwald.KzwMath;

namespace KaizerWald
{
    public static class StandardHungarianAlgorithm
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] StandardFindAssignments(float[] costs, int width)
        {
            int height = costs.Length / width;
            
            for (int y = 0; y < height; y++)
            {
                float minValue = float.MaxValue;
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(x, y, width);
                    minValue = min(minValue, costs[index]);
                }
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(x, y, width);
                    costs[index] -= minValue;
                }
            }
            
            byte[] masks = new byte[costs.Length];
            NativeArray<bool> rowsCovered = new (height,Temp, ClearMemory);
            NativeArray<bool> colsCovered = new (width,Temp, ClearMemory);

            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (colsCovered[x] || rowsCovered[y] || !costs[i].IsZero()) continue;
                masks[i] = 1;
                rowsCovered[y] = true;
                colsCovered[x] = true;
            }
            ClearCovers(rowsCovered, colsCovered, width, height);

            NativeArray<int2> path = new (costs.Length, Temp, ClearMemory);
            int2 pathStart = int2.zero;
            
            int step = 1;
            while (step != -1)
            {
                step = step switch
                {
                    1 => RunStep1(masks, colsCovered, width, height),
                    2 => RunStep2(costs, masks, rowsCovered, colsCovered, width, ref pathStart),
                    3 => RunStep3(masks, rowsCovered, colsCovered, width, height, path, pathStart),
                    4 => RunStep4(costs, rowsCovered, colsCovered, width),
                    _ => step
                };
            }
            
            //CANT be on single array! we search for each row, a valid assignation
            //by doing a single Array we may break BEFORE
            
            // !!! DONT SINGLE ARRAY THIS !!!
            int[] agentsTasks = new int[height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(x, y, width);
                    if (masks[index] != 1) continue;
                    agentsTasks[y] = x;
                    break;
                }
            }
            return agentsTasks;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RunStep1(byte[] masks, NativeArray<bool> colsCovered, int width, int height)
        {
            //CAREFULL sometimes: experiece wird behaviours:
            // parfois les "leader" de regiment semblent interverti
            for (int i = 0; i < masks.Length; i++)
            {
                if (masks[i] != 1) continue;
                int2 coords = GetXY2(i, width);
                colsCovered[coords.x] = true;
            }
            
            int colsCoveredCount = 0;
            for (int x = 0; x < width; x++)
            {
                colsCoveredCount += colsCovered[x] ? 1 : 0;
            }
            return colsCoveredCount == height ? -1 : 2;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RunStep2(float[] costs, byte[] masks, NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width, ref int2 pathStart)
        {
            while (true)
            {
                int2 location = FindZero(costs, rowsCovered, colsCovered, width);
                if (location.y == -1) return 4;
                masks[GetIndex(location, width)] = 2;

                int starColumn = FindStarInRow(masks, width, location.y);
                if (starColumn != -1)
                {
                    rowsCovered[location.y] = true;
                    colsCovered[starColumn] = false;
                }
                else
                {
                    pathStart = location;
                    return 3;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RunStep3(byte[] masks, NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width, int height, NativeArray<int2> path, int2 pathStart)
        {
            int pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                int y = FindStarInColumn(masks, path[pathIndex].x, width, height);
                if (y == -1) break;
                pathIndex++;
                path[pathIndex] = int2(path[pathIndex - 1].x, y); // path[pathIndex] = new Location(row, path[pathIndex - 1].x);
                int x = FindPrimeInRow(masks, width, path[pathIndex].y);
                pathIndex++;
                path[pathIndex] = int2(x, path[pathIndex - 1].y); // path[pathIndex] = new Location(path[pathIndex - 1].y, col);
            }
            ConvertPath(masks, path, pathIndex + 1, width);
            ClearCovers(rowsCovered, colsCovered, width, height);
            ClearPrimes(masks);
            return 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RunStep4(float[] costs, NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width)
        {
            float minValue = FindMinimum(costs, rowsCovered, colsCovered, width);
            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                costs[i] += rowsCovered[y] ? minValue : 0;
                costs[i] -= !colsCovered[x] ? minValue : 0;
            }
            return 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float FindMinimum(float[] costs, NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width)
        {
            float minValue = float.MaxValue;
            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (rowsCovered[y] || colsCovered[x]) continue;
                minValue = min(minValue, costs[i]);
            }
            return minValue;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindStarInRow(byte[] masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                int index = GetIndex(x, y, width);
                if (masks[index] == 1) return x;
            }
            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindStarInColumn(byte[] masks, int x, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                int index = GetIndex(x, y, width);
                if (masks[index] == 1) return y;
            }
            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindPrimeInRow(byte[] masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                int index = GetIndex(x, y, width);
                if (masks[index] == 2) return x;
            }
            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int2 FindZero(float[] costs, NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width)
        {
            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (colsCovered[x] || rowsCovered[y] || !costs[i].IsZero()) continue;
                return int2(x, y);
            }
            return int2(-1, -1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ConvertPath(byte[] masks, NativeArray<int2> path, int pathLength, int width)
        {
            for (int i = 0; i < pathLength; i++)
            {
                int index = GetIndex(path[i], width);
                masks[index] = masks[index] switch
                {
                    1 => 0,
                    2 => 1,
                    _ => masks[index]
                };
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ClearPrimes(byte[] masks)
        {
            for (int i = 0; i < masks.Length; i++)
            {
                if (masks[i] == 2) masks[i] = 0;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ClearCovers(NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                rowsCovered[i] = false;
                colsCovered[i] = false;
            }
            //for (int y = 0; y < height; y++) { rowsCovered[y] = false; }
            //for (int x = 0; x < width; x++) { colsCovered[x] = false; }
        }
    }
}
