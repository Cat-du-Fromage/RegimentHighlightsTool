using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaizerwald;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

using static Kaizerwald.KzwMath;

namespace Kaizerwald
{
    public static class NativeHungarianAlgorithm
    {
        public static int[] NativeFindAssignments(this NativeArray<int> costs, int width)
        {
            int height = costs.Length / width;

            //for (int i = 0; i < costs.Length; i++) { (int x, int y) = KzwMath.GetXY(i, width); }

            for (int y = 0; y < height; y++)
            {
                int minValue = int.MaxValue;
                for (int x = 0; x < width; x++)
                {
                    minValue = min(minValue, costs[GetIndex(x,y,width)]);
                }
                for (int x = 0; x < width; x++)
                {
                    costs[GetIndex(x,y,width)] = costs[GetIndex(x,y,width)] - minValue;
                }
            }

            byte[,] masks = new byte[height, width];
            bool[] rowsCovered = new bool[height];
            bool[] colsCovered = new bool[width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (costs[GetIndex(x,y,width)] == 0 && !rowsCovered[y] && !colsCovered[x])
                    {
                        masks[y, x] = 1;
                        rowsCovered[y] = true;
                        colsCovered[x] = true;
                    }
                }
            }

            ClearCovers(rowsCovered, colsCovered, width, height);

            Location[] path = new Location[width * height];
            Location pathStart = default(Location);
            int step = 1;

            while (step != -1)
            {
                step = step switch
                {
                    1 => RunStep1(masks, colsCovered, width, height),
                    2 => RunStep2(costs, masks, rowsCovered, colsCovered, width, height, ref pathStart),
                    3 => RunStep3(masks, rowsCovered, colsCovered, width, height, path, pathStart),
                    4 => RunStep4(costs, rowsCovered, colsCovered, width, height),
                    _ => step
                };
            }

            int[] agentsTasks = new int[height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (masks[y, x] != 1) continue;
                    agentsTasks[y] = x;
                    break;
                }
            }
            return agentsTasks;
        }
        
        private static int RunStep1(byte[,] masks, bool[] xCovered, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (masks[y, x] == 1)
                    {
                        xCovered[x] = true;
                    }
                }
            }

            int colsCoveredCount = 0;

            for (int x = 0; x < width; x++)
            {
                //if (colsCovered[x]) colsCoveredCount++;
                colsCoveredCount += xCovered[x] ? 1 : 0;
            }

            return colsCoveredCount == height ? -1 : 2;
            //if (colsCoveredCount == height) return -1;
            //return 2;
        }
        
        private static int RunStep2(NativeArray<int> costs, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int width, int height, ref Location pathStart)
        {
            while (true)
            {
                Location loc = FindZero(costs, rowsCovered, colsCovered, width, height);
                if (loc.row == -1) return 4;

                masks[loc.row, loc.column] = 2;

                int starCol = FindStarInRow(masks, width, loc.row);
                if (starCol != -1)
                {
                    rowsCovered[loc.row] = true;
                    colsCovered[starCol] = false;
                }
                else
                {
                    pathStart = loc;
                    return 3;
                }
            }
        }
        
        private static int RunStep3(byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, Location[] path, Location pathStart)
        {
            int pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                int row = FindStarInColumn(masks, h, path[pathIndex].column);
                if (row == -1) break;
                pathIndex++;
                path[pathIndex] = new Location(row, path[pathIndex - 1].column);

                int col = FindPrimeInRow(masks, w, path[pathIndex].row);
                pathIndex++;
                path[pathIndex] = new Location(path[pathIndex - 1].row, col);
            }
            ConvertPath(masks, path, pathIndex + 1);
            ClearCovers(rowsCovered, colsCovered, w, h);
            ClearPrimes(masks, w, h);
            return 1;
        }
        
        private static int RunStep4(NativeArray<int> costs, bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            int minValue = FindMinimum(costs, rowsCovered, colsCovered, width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //if (rowsCovered[y])
                    int toAdd = rowsCovered[y] ? minValue : 0;
                    costs[GetIndex(x,y,width)] = costs[GetIndex(x,y,width)] + toAdd;
                    //if (!colsCovered[x])
                    int toRemove = !colsCovered[x] ? minValue : 0;
                    costs[GetIndex(x,y,width)] = costs[GetIndex(x,y,width)] - toRemove;
                }
            }
            return 2;
        }

        private static int FindMinimum(NativeArray<int> costs, bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            int minValue = int.MaxValue;
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
            return minValue;
        }
        
        private static int FindStarInRow(byte[,] masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                if (masks[y, x] == 1) return x;
            }
            return -1;
        }
        
        private static int FindStarInColumn(byte[,] masks, int height, int x)
        {
            for (int y = 0; y < height; y++)
            {
                if (masks[y, x] == 1) return y;
            }
            return -1;
        }
        
        private static int FindPrimeInRow(byte[,] masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                if (masks[y, x] == 2) return x;
            }
            return -1;
        }
        private static Location FindZero(NativeArray<int> costs, bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (costs[GetIndex(x,y,width)] == 0 && !rowsCovered[y] && !colsCovered[x])
                        return new Location(y, x);
                }
            }
            return new Location(-1, -1);
        }
        private static void ConvertPath(byte[,] masks, Location[] path, int pathLength)
        {
            for (int i = 0; i < pathLength; i++)
            {
                masks[path[i].row, path[i].column] = masks[path[i].row, path[i].column] switch
                {
                    1 => 0,
                    2 => 1,
                    _ => masks[path[i].row, path[i].column]
                };
            }
        }
        
        private static void ClearPrimes(byte[,] masks, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (masks[y, x] == 2) masks[y, x] = 0;
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

        private struct Location
        {
            internal readonly int row;
            internal readonly int column;

            internal Location(int row, int col)
            {
                this.row = row;
                this.column = col;
            }
        }
    }
}