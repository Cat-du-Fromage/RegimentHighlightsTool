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

using Kaizerwald;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using static Kaizerwald.KzwMath;
using Debug = UnityEngine.Debug;

namespace Kaizerwald
{
    public static class JobifiedHungarian2
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<int> FindAssignments(NativeArray<float> costs, int width)
        {
            int height = costs.Length / width;
            
            FindAndSubtractRowMinValue(costs, width, height);
            
            NativeArray<byte> masks       = new (costs.Length, TempJob, ClearMemory);
            NativeArray<bool> rowsCovered = new (height,TempJob, ClearMemory);
            NativeArray<bool> colsCovered = new (width,TempJob, ClearMemory);

            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (!costs[i].IsZero() || rowsCovered[y] || colsCovered[x]) continue;
                masks[i] = 1;
                rowsCovered[y] = true;
                colsCovered[x] = true;
            }
            ClearCovers(rowsCovered, colsCovered, width, height);

            NativeArray<int2> path = new (costs.Length, TempJob, ClearMemory);
            NativeArray<int> agentTasks = new (width, TempJob, UninitializedMemory);
            JHungarianAlgorithmSteps job = new JHungarianAlgorithmSteps
            {
                Width = width,
                Height = height,
                PathStart = int2.zero,
                Costs = costs,
                Masks = masks,
                RowsCovered = rowsCovered,
                ColsCovered = colsCovered,
                Path = path,
                AgentsTasks = agentTasks
            };
            JobHandle jobHandle = job.Schedule(default);
            
            masks.Dispose(jobHandle);
            rowsCovered.Dispose(jobHandle);
            colsCovered.Dispose(jobHandle);
            path.Dispose(jobHandle);
            
            jobHandle.Complete();
            
            return agentTasks;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ JOBS ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FindAndSubtractRowMinValue(NativeArray<float> costs, int width, int height)
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
        private static void ClearCovers(NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, int width, int height)
        {
            for (int y = 0; y < height; y++) { rowsCovered[y] = false; }
            for (int x = 0; x < width; x++) { colsCovered[x] = false; }
        }
        
        /*
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
        */
    }
    
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ JOBS ◆◆◆◆◆◆                                                    ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //[BurstCompile]
        public struct JHungarianAlgorithmSteps : IJob
        {
            public int Width;
            public int Height;

            public int2 PathStart;
            
            public NativeArray<float> Costs;
            public NativeArray<byte> Masks;
            public NativeArray<bool> RowsCovered;
            public NativeArray<bool> ColsCovered;

            public NativeArray<int2> Path;
            public NativeArray<int> AgentsTasks;
            
            public void Execute()
            {
                int step = 1;
                while (step != -1)// <-- THIS WHILE is ne one crashing
                {
                    if (step == 1)
                    {
                        RunStep1(ref step);
                    }
                    else if (step == 2)
                    {
                        RunStep2(ref step);
                    }
                    else if (step == 3)
                    {
                        RunStep3(ref step);
                    }
                    else if (step == 4)
                    {
                        RunStep4(ref step);
                    }
                    else
                    {
                        break;
                    }
                }
                
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int index = y * Width + x;
                        if (Masks[index] != 1) continue;
                        AgentsTasks[y] = x;
                        break;
                    }
                }
            }
            
            private int2 FindZero()
            {
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    if (ColsCovered[x] || RowsCovered[y] || !Mathf.Approximately(Costs[i],0)) continue;
                    return new int2(x, y);
                }
                return new int2(-1, -1);
            }
            
            private void RunStep1(ref int step)
            {
                for (int i = 0; i < Masks.Length; i++)
                {
                    if (Masks[i] != 1) continue;
                    int x = i % Width;
                    ColsCovered[x] = true;
                }
                int colsCoveredCount = 0;
                for (int x = 0; x < Width; x++)
                {
                    colsCoveredCount = colsCoveredCount + select(0,1,ColsCovered[x]);
                }
                step = colsCoveredCount == Height ? -1 : 2;
                //return colsCoveredCount == Height ? -1 : 2;
            }
            
            private void RunStep2(ref int step)
            {
                while (true)
                {
                    int2 location = FindZero();
                    if (location.y == -1)
                    {
                        step = 4;
                        return;
                        //return 4;
                    }
                    int index = location.y * Width + location.x;
                    Masks[index] = 2;

                    int starColumn = FindStarInRow(location.y);
                    if (starColumn != -1)
                    {
                        RowsCovered[location.y] = true;
                        ColsCovered[starColumn] = false;
                    }
                    else
                    {
                        PathStart = location;
                        step = 3;
                        return;
                        //return 3;
                    }
                }
            }
            
            private void RunStep3(ref int step)
            {
                int pathIndex = 0;
                Path[0] = PathStart;
                while (true)
                {
                    int y = FindStarInColumn(Path[pathIndex].x);
                    if (y == -1) break;
                    pathIndex += 1;
                    Path[pathIndex] = new int2(Path[pathIndex - 1].x, y);
                    int x = FindPrimeInRow(Path[pathIndex].y);
                    pathIndex += 1;
                    Path[pathIndex] = new int2(x, Path[pathIndex - 1].y);
                }
                ConvertPath(pathIndex + 1);
                ClearCovers();
                ClearPrimes();
                step = 1;
            }
            
            private void RunStep4(ref int step)
            {
                float minValue = float.MaxValue;
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    if (RowsCovered[y] || ColsCovered[x]) continue;
                    minValue = min(minValue, Costs[i]);
                }
                
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    float currentCost = Costs[i];
                    Costs[i] = currentCost + select(0,minValue,RowsCovered[y]);
                    currentCost = Costs[i];
                    Costs[i] = currentCost - select(0,minValue,!ColsCovered[x]);
                }
                step = 2;
            }
            
            private void ConvertPath(int pathLength)
            {
                for (int i = 0; i < pathLength; i++)
                {
                    int index = Path[i].y * Width + Path[i].x;
                    byte maskValue = Masks[index];
                    if (maskValue == 1)
                    {
                        Masks[index] = 0;
                    }
                    else if (Masks[index] == 2)
                    {
                        Masks[index] = 1;
                    }
                }
            }
            
            private void ClearCovers()
            {
                for (int y = 0; y < Height; y++) { RowsCovered[y] = false; }
                for (int x = 0; x < Width; x++) { ColsCovered[x] = false; }
            }
            
            private void ClearPrimes()
            {
                for (int i = 0; i < Masks.Length; i++)
                {
                    if (Masks[i] == 2) Masks[i] = 0;
                }
            }
            
            private int FindStarInRow(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 1) return x;
                }
                return -1;
            }
            
            private int FindPrimeInRow(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 2) return x;
                }
                return -1;
            }
            
            private int FindStarInColumn(int x)
            {
                for (int y = 0; y < Height; y++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 1) return y;
                }
                return -1;
            }
        }
}
