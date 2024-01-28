using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Kaizerwald;
using static Kaizerwald.KzwMath;

namespace Kaizerwald
{
    public static class NativeHungarianAlgorithm
    {
        public static int[] NativeFindAssignments(this NativeArray<float> costs, int width)
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
                    float newCost = costs[index] - minValue;
                    costs[index] = newCost;
                }
            }
            
            NativeArray<byte> masks = new (costs.Length, Temp, NativeArrayOptions.ClearMemory);
            bool[] rowsCovered = new bool[height];
            bool[] colsCovered = new bool[width];

            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (colsCovered[x] || rowsCovered[y] || !costs[i].IsZero()) continue;
                masks[i] = 1;
                rowsCovered[y] = true;
                colsCovered[x] = true;
            }
            ClearCovers(rowsCovered, colsCovered, width, height);

            int2[] path = new int2[costs.Length];
            int2 pathStart = default;
            
            int step = 1;
            while (step != -1)
            {
                step = step switch
                {
                    1 => RunStep1(ref masks, colsCovered, width, height),
                    2 => RunStep2(ref costs, ref masks, rowsCovered, colsCovered, width, height, ref pathStart),
                    3 => RunStep3(ref masks, rowsCovered, colsCovered, width, height, path, pathStart),
                    4 => RunStep4(ref costs, rowsCovered, colsCovered, width, height),
                    _ => step
                };
            }

            int[] agentsTasks = new int[height];
            
            //CANT be on single array! we search for each row, a valid assignation
            //by doing a single Array we may break BEFORE
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
        
        private static int RunStep1(ref NativeArray<byte> masks, bool[] xCovered, int width, int height)
        {
            //CAREFULL sometimes: experiece wird behaviours:
            // parfois les "leader" de regiment semblent interverti
            for (int i = 0; i < masks.Length; i++)
            {
                if (masks[i] != 1) continue;
                int2 coords = GetXY2(i, width);
                xCovered[coords.x] = true;
            }
            
            /*
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (masks[GetIndex(x,y,width)] == 1)
                    {
                        xCovered[x] = true;
                    }
                }
            }
            */
            
            int colsCoveredCount = 0;
            for (int x = 0; x < width; x++)
            {
                colsCoveredCount += xCovered[x] ? 1 : 0;
            }
            return colsCoveredCount == height ? -1 : 2;
        }
        
        private static int RunStep2(ref NativeArray<float> costs, ref NativeArray<byte> masks, bool[] rowsCovered, bool[] colsCovered, int width, int height, ref int2 pathStart)
        {
            while (true)
            {
                int2 loc = FindZero(costs, rowsCovered, colsCovered, width, height);
                if (loc.y == -1) return 4;

                int index = GetIndex(loc, width);
                masks[index] = 2;
                //masks[loc.y, loc.x] = 2;

                int starCol = FindStarInRow(masks, width, loc.y);
                if (starCol != -1)
                {
                    rowsCovered[loc.y] = true;
                    colsCovered[starCol] = false;
                }
                else
                {
                    pathStart = loc;
                    return 3;
                }
            }
        }
        
        private static int RunStep3(ref NativeArray<byte> masks, bool[] rowsCovered, bool[] colsCovered, int width, int height, int2[] path, int2 pathStart)
        {
            int pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                int row = FindStarInColumn(masks, width, height, path[pathIndex].x);
                if (row == -1) break;
                pathIndex++;
                path[pathIndex] = new int2(path[pathIndex - 1].x, row); // path[pathIndex] = new Location(row, path[pathIndex - 1].x);
                int col = FindPrimeInRow(masks, width, path[pathIndex].y);
                pathIndex++;
                path[pathIndex] = new int2(col, path[pathIndex - 1].y); // path[pathIndex] = new Location(path[pathIndex - 1].y, col);
            }
            ConvertPath(ref masks, path, pathIndex + 1, width);
            ClearCovers(rowsCovered, colsCovered, width, height);
            ClearPrimes(ref masks, width, height);
            return 1;
        }
        
        private static int RunStep4(ref NativeArray<float> costs, bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            float minValue = FindMinimum(costs, rowsCovered, colsCovered, width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(x, y, width);
                    //if (rowsCovered[y])
                    float added = rowsCovered[y] ? minValue : 0;
                    costs[index] = costs[index] + added;
                    //if (!colsCovered[x])
                    float subtract = !colsCovered[x] ? minValue : 0;
                    costs[index] = costs[index] - subtract;
                }
            }
            return 2;
        }

        private static float FindMinimum(in NativeArray<float> costs, bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            float minValue = float.MaxValue;
            /*
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!rowsCovered[y] && !colsCovered[x])
                    {
                        minValue = min(minValue, costs[GetIndex(x,y,width)]);
                    }
                }
            }
            */
            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (!rowsCovered[y] && !colsCovered[x])
                {
                    minValue = min(minValue, costs[i]);
                }
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
        
        private static int FindStarInColumn(in NativeArray<byte> masks, int width, int height, int x)
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
        private static int2 FindZero(in NativeArray<float> costs, bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(x, y, width);
                    //if (costs[index] == 0 && !rowsCovered[y] && !colsCovered[x]) return new int2(x, y);
                    if (costs[index].IsZero() && !rowsCovered[y] && !colsCovered[x]) return new int2(x, y);
                }
            }
            return new int2(-1, -1);
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
                /*
                masks[path[i].y, path[i].x] = masks[path[i].y, path[i].x] switch
                {
                    1 => 0,
                    2 => 1,
                    _ => masks[path[i].y, path[i].x]
                };
                */
            }
        }
        
        private static void ClearPrimes(ref NativeArray<byte> masks, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(x, y, width);
                    if (masks[index] == 2) masks[index] = 0;
                }
            }
        }
        
        private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            //Array.Fill(rowsCovered, false);
            //Array.Fill(colsCovered, false);
            for (int y = 0; y < height; y++) { rowsCovered[y] = false; }
            for (int x = 0; x < width; x++) { colsCovered[x] = false; }
        }
    }
}
