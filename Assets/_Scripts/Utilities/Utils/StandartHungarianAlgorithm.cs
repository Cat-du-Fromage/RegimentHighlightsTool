using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using int2 = Unity.Mathematics.int2;
using Debug = UnityEngine.Debug;

using Kaizerwald;
using static Kaizerwald.KzwMath;


namespace Kaizerwald
{
    
    public static class StandardHungarianAlgorithm
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] StandardFindAssignments(float[] costs, int width)
        {
            int height = costs.Length / width;
            
            FindAndSubtractRowMinValue(costs, width, height);
            
            byte[] masks = new byte[costs.Length];
            bool[] rowsCovered = new bool[height];
            bool[] colsCovered = new bool[width];

            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (!costs[i].IsZero() || rowsCovered[y] || colsCovered[x]) continue;
                masks[i] = 1;
                rowsCovered[y] = true;
                colsCovered[x] = true;
            }
            ClearCovers(rowsCovered, colsCovered, width, height);

            int2[] path = new int2[costs.Length];
            (int step, int2 pathStart) = (1, int2.zero);
            
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
        private static void FindAndSubtractRowMinValue(float[] costs, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                float minValue = FindRowMinValue(y);
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(x, y, width);
                    costs[index] -= minValue;
                }
            }

            return;
            float FindRowMinValue(int y)
            {
                float minValue = float.MaxValue;
                for (int x = 0; x < width; x++)
                {
                    int index = GetIndex(x, y, width);
                    minValue = min(minValue, costs[index]);
                }
                return minValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RunStep1(byte[] masks, bool[] colsCovered, int width, int height)
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
        private static int RunStep2(float[] costs, byte[] masks, bool[] rowsCovered, bool[] colsCovered, int width, ref int2 pathStart)
        {
            while (true)
            {
                int2 location = FindZero(costs, rowsCovered, colsCovered, width);
                if (location.y == -1)
                {
                    return 4;
                }
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
        private static int RunStep3(byte[] masks, bool[] rowsCovered, bool[] colsCovered, int width, int height, int2[] path, int2 pathStart)
        {
            int pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                int y = FindStarInColumn(masks, path[pathIndex].x, width, height);
                if (y == -1) break;
                pathIndex++;
                path[pathIndex] = int2(path[pathIndex - 1].x, y);
                int x = FindPrimeInRow(masks, width, path[pathIndex].y);
                pathIndex++;
                path[pathIndex] = int2(x, path[pathIndex - 1].y);
            }
            ConvertPath(masks, path, pathIndex + 1, width);
            ClearCovers(rowsCovered, colsCovered, width, height);
            ClearPrimes(masks);
            return 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RunStep4(float[] costs, bool[] rowsCovered, bool[] colsCovered, int width)
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
        private static float FindMinimum(float[] costs, bool[] rowsCovered, bool[] colsCovered, int width)
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
        private static int2 FindZero(float[] costs, bool[] rowsCovered, bool[] colsCovered, int width)
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
        private static void ConvertPath(byte[] masks, int2[] path, int pathLength, int width)
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
        private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            Array.Fill(rowsCovered, false);
            Array.Fill(colsCovered, false);
            //for (int y = 0; y < height; y++) { rowsCovered[y] = false; }
            //for (int x = 0; x < width; x++) { colsCovered[x] = false; }
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ Test2 ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] StandardFindAssignments2(float[] costs, int width)
        {
            int height = costs.Length / width;
            
            FindAndSubtractRowMinValue(costs, width, height);
            
            byte[] masks = new byte[costs.Length];
            bool[] rowsCovered = new bool[height];
            bool[] colsCovered = new bool[width];

            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (!costs[i].IsZero() || rowsCovered[y] || colsCovered[x]) continue;
                masks[i] = 1;
                rowsCovered[y] = true;
                colsCovered[x] = true;
            }
            ClearCovers(rowsCovered, colsCovered, width, height);

            int2[] path = new int2[costs.Length];
            (int step, int2 pathStart) = (1, int2.zero);
            
            while (step != -1)
            {
                step = step switch
                {
                    1 => InternalRunStep1(),
                    2 => InternalRunStep2(),
                    3 => InternalRunStep3(),
                    4 => InternalRunStep4(),
                    _ => step
                };
            }
            
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
            
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Utils ◈◈◈◈◈◈                                                                                        ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
            void InternalClearCovers()
            {
                Array.Fill(rowsCovered, false);
                Array.Fill(colsCovered, false);
            }
            
            int2 InternalFindZero()
            {
                for (int i = 0; i < costs.Length; i++)
                {
                    int2 coords = GetXY2(i, width);
                    if (colsCovered[coords.x] || rowsCovered[coords.y] || !costs[i].IsZero()) continue;
                    return coords;
                }
                return new int2(-1, -1);
            }
            
            float InternalFindMinimum()
            {
                float minValue = float.MaxValue;
                for (int i = 0; i < costs.Length; i++)
                {
                    (int x, int y) = GetXY(i, width);
                    //if (rowsCovered[y] || colsCovered[x]) continue;
                    minValue = rowsCovered[y] || colsCovered[x] ? minValue : min(minValue, costs[i]);
                }
                return minValue;
            }
            
            void InternalClearPrimes()
            {
                for (int i = 0; i < masks.Length; i++)
                {
                    masks[i] = (byte)(masks[i] == 2 ? 0 : masks[i]);
                }
            }
            
            void InternalConvertPath(int pathLength)
            {
                for (int i = 0; i < pathLength; i++)
                {
                    int index = GetIndex(path[i], width);
                    //if (masks[index] is < 1 or > 2) continue;
                    //masks[index] = (byte)(masks[index] == 1 ? 0 : 1);
                    masks[index] = masks[index] switch
                    {
                        1 => 0,
                        2 => 1,
                        _ => masks[index]
                    };
                }
            }
            
            int InternalFindPrimeInRow(int y)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    if (masks[index] != 2) continue;
                    return x;
                }
                return -1;
            }
            
            int InternalFindStarInRow(int y)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    if (masks[index] != 1) continue;
                    return x;
                }
                return -1;
            }
            
            int InternalFindStarInColumn(int x)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = y * width + x;
                    if (masks[index] != 1) continue;
                    return y;
                }
                return -1;
            }
            
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Steps ◈◈◈◈◈◈                                                                                        ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
            int InternalRunStep1()
            {
                for (int i = 0; i < masks.Length; i++)
                {
                    if (masks[i] != 1) continue;
                    int x = i % width;
                    colsCovered[x] = true;
                }
                
                int colsCoveredCount = 0;
                for (int x = 0; x < width; x++)
                {
                    colsCoveredCount += colsCovered[x] ? 1 : 0;
                }
                return colsCoveredCount == height ? -1 : 2;
            }
        
            int InternalRunStep2()
            {
                while (true)
                {
                    int2 location = InternalFindZero();
                    if (location.y == -1) return 4;
                    masks[GetIndex(location, width)] = 2;
                    int starColumn = InternalFindStarInRow(location.y);
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
            
            int InternalRunStep3()
            {
                int pathIndex = 0;
                path[0] = pathStart;
                while (true)
                {
                    int y = InternalFindStarInColumn(path[pathIndex].x);
                    if (y == -1) break;
                    path[++pathIndex] = new int2(path[pathIndex - 1].x, y);
                    int x = InternalFindPrimeInRow(path[pathIndex].y);
                    path[++pathIndex] = new int2(x, path[pathIndex - 1].y);
                }
                InternalConvertPath(pathIndex + 1);
                InternalClearCovers();
                InternalClearPrimes();
                return 1;
            }
            
            int InternalRunStep4()
            {
                float minValue = InternalFindMinimum();
                for (int i = 0; i < costs.Length; i++)
                {
                    (int x, int y) = GetXY(i, width);
                    costs[i] += rowsCovered[y] ? minValue : 0;
                    costs[i] -= !colsCovered[x] ? minValue : 0;
                }
                return 2;
            }
        }
    }
}
