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

namespace Kaizerwald
{
    public static class NativeHungarianAlgorithm
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float FindMinValue(in NativeSlice<float> row)
        {
            float minValue = float.MaxValue;
            for (int i = 0; i < row.Length; i++)
            {
                minValue = min(minValue, row[i]);
            }
            return minValue;
        }
        
        public static int[] NativeFindAssignments(this NativeArray<float> costs, int width)
        {
            int height = costs.Length / width;
            
            for (int y = 0; y < height; y++)
            {
                int minIndex = y * width;
                float minValue = FindMinValue(costs.Slice(minIndex, width));
                for (int x = 0; x < width; x++)
                {
                    int currentIndex = minIndex + x;
                    costs[currentIndex] -= minValue;
                }
            }
            /*
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
                    float newCost = costs[index] - minValue;
                    costs[index] = newCost;
                }
            }
            */
            NativeArray<byte> masks = new (costs.Length, Temp, ClearMemory);
            NativeArray<bool> rowsCovered = new (height,Temp, ClearMemory);
            NativeArray<bool> colsCovered = new (height,Temp, ClearMemory);

            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (colsCovered[x] || rowsCovered[y] || !costs[i].IsZero()) continue;
                masks[i] = 1;
                rowsCovered[y] = true;
                colsCovered[x] = true;
            }
            ClearCovers(ref rowsCovered, ref colsCovered, width, height);

            int2[] path = new int2[costs.Length];
            int2 pathStart = int2.zero;
            
            int step = 1;
            while (step != -1)
            {
                step = step switch
                {
                    1 => RunStep1(ref masks, ref colsCovered, width, height),
                    2 => RunStep2(ref costs, ref masks, ref rowsCovered, ref colsCovered, width, ref pathStart),
                    3 => RunStep3(ref masks, ref rowsCovered, ref colsCovered, width, height, path, pathStart),
                    4 => RunStep4(ref costs, rowsCovered, colsCovered, width, height),
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
        
        private static int RunStep1(ref NativeArray<byte> masks, ref NativeArray<bool> colsCovered, int width, int height)
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
        
        private static int RunStep2(ref NativeArray<float> costs, ref NativeArray<byte> masks, ref NativeArray<bool> rowsCovered, ref NativeArray<bool> colsCovered, int width, ref int2 pathStart)
        {
            while (true)
            {
                int2 location = FindZero(costs, rowsCovered, colsCovered, width);
                if (location.y == -1) return 4;
                int index = GetIndex(location, width);
                masks[index] = 2;

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
        
        private static int RunStep3(ref NativeArray<byte> masks, ref NativeArray<bool> rowsCovered, ref NativeArray<bool> colsCovered, int width, int height, int2[] path, int2 pathStart)
        {
            int pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                int row = FindStarInColumn(masks, path[pathIndex].x, width, height);
                if (row == -1) break;
                pathIndex++;
                path[pathIndex] = new int2(path[pathIndex - 1].x, row); // path[pathIndex] = new Location(row, path[pathIndex - 1].x);
                int col = FindPrimeInRow(masks, width, path[pathIndex].y);
                pathIndex++;
                path[pathIndex] = new int2(col, path[pathIndex - 1].y); // path[pathIndex] = new Location(path[pathIndex - 1].y, col);
            }
            ConvertPath(ref masks, path, pathIndex + 1, width);
            ClearCovers(ref rowsCovered, ref colsCovered, width, height);
            ClearPrimes(ref masks);
            return 1;
        }
        
        private static int RunStep4(ref NativeArray<float> costs, NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width, int height)
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

        private static float FindMinimum(in NativeArray<float> costs, NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width)
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
        
        private static int FindStarInRow(in NativeArray<byte> masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                int index = GetIndex(x, y, width);
                if (masks[index] == 1) return x;
            }
            return -1;
        }
        
        private static int FindStarInColumn(in NativeArray<byte> masks, int x, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                int index = GetIndex(x, y, width);
                if (masks[index] == 1) return y;
            }
            return -1;
        }
        
        private static int FindPrimeInRow(in NativeArray<byte> masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                int index = GetIndex(x, y, width);
                if (masks[index] == 2) return x;
            }
            return -1;
        }
        private static int2 FindZero(in NativeArray<float> costs, in NativeArray<bool> rowsCovered, in NativeArray<bool> colsCovered, int width)
        {
            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (colsCovered[x] || rowsCovered[y] || !costs[i].IsZero()) continue;
                return int2(x, y);
            }
            return int2(-1, -1);
        }
        private static void ConvertPath(ref NativeArray<byte> masks, int2[] path, int pathLength, int width)
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
        
        private static void ClearPrimes(ref NativeArray<byte> masks)
        {
            for (int i = 0; i < masks.Length; i++)
            {
                if (masks[i] == 2) masks[i] = 0;
            }
        }
        
        private static void ClearCovers(ref NativeArray<bool> rowsCovered, ref NativeArray<bool> colsCovered, int width, int height)
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
