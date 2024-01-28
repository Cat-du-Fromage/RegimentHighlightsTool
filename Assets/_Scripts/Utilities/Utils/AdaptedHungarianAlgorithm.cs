using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Mathematics.math;

using static Kaizerwald.KzwMath;

namespace Kaizerwald
{
    public static class AdaptedHungarianAlgorithm
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindMinValue(in NativeSlice<int> row)
        {
            int minValue = int.MaxValue;
            for (int i = 0; i < row.Length; i++)
            {
                minValue = min(minValue, row[i]);
            }
            return minValue;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int2 FindZero(in NativeArray<int> costs, in NativeArray<bool> yCovered, in NativeArray<bool> xCovered, int width)
        {
            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (costs[i] != 0 || yCovered[y] || xCovered[x]) continue;
                return new int2(x,y);
            }
            return new int2(-1, -1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindStarInRow(in NativeArray<byte> masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                int index = GetIndex(x,y,width);
                if (masks[index] == 1) return x;
            }
            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindStarInColumn(in NativeArray<byte> masks, int width, int height, int x)
        {
            for (int y = 0; y < height; y++)
            {
                int index = GetIndex(x,y,width);
                if (masks[index] == 1) return y;
            }
            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindPrimeInRow(in NativeArray<byte> masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                int index = GetIndex(x,y,width);
                if (masks[index] == 2) return x;
            }
            return -1;
        }
        
        private static void CreateMasksAndCoveredArrays(NativeArray<byte> masks, NativeArray<bool> rowsCovered, NativeArray<bool> colsCovered, NativeArray<int> costs, int width, int height)
        {
            for (int i = 0; i < masks.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (costs[i] == 0 && !rowsCovered[y] && !colsCovered[x])
                {
                    masks[i] = 1;
                    rowsCovered[y] = true;
                    colsCovered[x] = true;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ClearCovers(ref NativeArray<bool> yCovered, ref NativeArray<bool> xCovered, int width, int height)
        {
            for (int y = 0; y < height; y++) { yCovered[y] = false; }
            for (int x = 0; x < width; x++) { xCovered[x] = false; }
        }
        /*
        public static NativeArray<int> Setup(int amount, List<FormationSpot> spots, Vector3[] soldierPositions, Vector3 pivot, Vector3 formationCentre, Quaternion targetRotation, Vector3 unitPosition)
        {
            float[,] array = new float[amount, spots.Count];
            for (int i = 0; i < amount; i++)
            {
                for (int j = 0; j < spots.Count; j++)
                {
                    Vector3 b = targetRotation * (spots[j].position + (unitPosition - formationCentre) - pivot) + pivot;
                    float sqrMagnitude = (_soldierPositions[i] - b).sqrMagnitude;
                    array[i, j] = sqrMagnitude;
                }
            }
            return array;
        }
        */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<int> FindAssignments(NativeArray<int> costs, int width, Allocator allocator = Temp)
        {
            int height = costs.Length / width;
            
            /*
            for (int y = 0; y < height; y++)
            {
                int minIndex = y * width;
                int minValue = int.MaxValue;

                // Calculate minimum value within the row
                for (int x = 0; x < width; x++)
                {
                    int currentIndex = minIndex + x;
                    minValue = min(minValue, costs[currentIndex]);
                }

                // Subtract minimum value from each element in the row
                for (int x = 0; x < width; x++)
                {
                    int currentIndex = minIndex + x;
                    costs[currentIndex] -= minValue;
                }
            }
            */
            
            //Find And Subtract minimum value from each element in the row
            for (int y = 0; y < height; y++)
            {
                int minIndex = y * width;
                int minValue = FindMinValue(costs.Slice(minIndex, width));
                for (int x = 0; x < width; x++)
                {
                    int currentIndex = minIndex + x;
                    costs[currentIndex] = costs[currentIndex] - minValue;
                }
            }
            
            NativeArray<byte> masks = new (costs.Length, Temp, UninitializedMemory);
            NativeArray<bool> yCovered = new (height, Temp, UninitializedMemory);
            NativeArray<bool> xCovered = new (width, Temp, UninitializedMemory);
            
            // Methods: CreateMasksAndCoveredArrays
            for (int i = 0; i < masks.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (costs[i] != 0 || yCovered[y] || xCovered[x]) continue;
                masks[i] = 1;
                yCovered[y] = true;
                xCovered[x] = true;
            }
            
            ClearCovers(ref yCovered, ref xCovered, width, height);
            NativeArray<int2> path = new (costs.Length, Temp, UninitializedMemory);
            int2 pathStart = default;
            int step = 1;
            
            while (step != -1)
            {
                step = step switch
                {
                    1 => RunStep1(),
                    2 => RunStep2(),
                    3 => RunStep3(),
                    4 => RunStep4(),
                    _ => step
                };
            }
            
            NativeArray<int> agentsTasks = new (height, allocator, UninitializedMemory);
            for (int i = 0; i < masks.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                if (masks[i] != 1) continue;
                agentsTasks[y] = x;
                break;
            }
            
            return agentsTasks;
            //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
            //│  ◇◇◇◇◇◇ Internal Methods ◇◇◇◇◇◇                                                                            │
            //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
            int RunStep1()
            {
                for (int i = 0; i < masks.Length; i++)
                {
                    (int x, int _) = GetXY(i, width);
                    if (masks[i] == 1) xCovered[x] = true;
                }
                int colsCoveredCount = 0;
                for (int x = 0; x < width; x++)
                {
                    colsCoveredCount += xCovered[x] ? 1 : 0;
                }
                return colsCoveredCount == height ? -1 : 2;
            }
            
            int RunStep2()
            {
                while (true)
                {
                    int2 loc = FindZero(costs, yCovered, xCovered, width);
                    if (loc.y == -1) return 4;
                    int index = GetIndex(loc, width);
                    masks[index] = 2;
                    int starCol = FindStarInRow(masks, width, loc.y);
                    if (starCol != -1)
                    {
                        yCovered[loc.y] = true;
                        xCovered[starCol] = false;
                    }
                    else
                    {
                        pathStart = loc;
                        return 3;
                    }
                }
            }
            int RunStep3()
            {
                int pathIndex = 0;
                path[0] = pathStart;

                while (true)
                {
                    int row = FindStarInColumn(masks, height, width,path[pathIndex].x);
                    if (row == -1) break;
                    pathIndex++;
                    path[pathIndex] = new int2(row, path[pathIndex - 1].x);

                    int col = FindPrimeInRow(masks, width, path[pathIndex].y);
                    pathIndex++;
                    path[pathIndex] = new int2(path[pathIndex - 1].y, col);
                }
                // ===========================================================
                // ConvertPath(masks, path, pathIndex + 1, width);
                int pathLength = pathIndex + 1;
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
                // ===========================================================
                
                ClearCovers(ref yCovered, ref xCovered, width, height);
                
                // ===========================================================
                // ClearPrimes(masks);
                for (int i = 0; i < masks.Length; i++)
                {
                    //if (masks[i] == 2) masks[i] = 0;
                    masks[i] = masks[i] == 2 ? (byte)0 : masks[i];
                }
                // ===========================================================
                return 1;
            }
            
            int RunStep4()
            {
                int minValue = FindMinimum(costs, yCovered, xCovered, width);
                for (int i = 0; i < costs.Length; i++)
                {
                    (int x, int y) = GetXY(i, width);
                    int toAdd = yCovered[y] ? minValue : 0;
                    int toRemove = !xCovered[x] ? minValue : 0;
                    costs[i] = costs[i] + toAdd;
                    costs[i] = costs[i] - toRemove;
                }
                return 2;
            }
        }
        /*
        private static int RunStep1(NativeArray<byte> masks, NativeArray<bool> xCovered, int width, int height)
        {
            for (int i = 0; i < masks.Length; i++)
            {
                (int x, int y) = KzwMath.GetXY(i, width);
                if (masks[i] == 1) xCovered[x] = true;
            }

            int colsCoveredCount = 0;
            for (int x = 0; x < width; x++)
            {
                colsCoveredCount += xCovered[x] ? 1 : 0;
            }

            return colsCoveredCount == height ? -1 : 2;
        }
    
        private static int RunStep2(NativeArray<int> costs, NativeArray<byte> masks, NativeArray<bool> yCovered, NativeArray<bool> xCovered, int width, ref int2 pathStart)
        {
            while (true)
            {
                int2 loc = FindZero(costs, yCovered, xCovered, width);
                if (loc.y == -1) return 4;
                int index = KzwMath.GetIndex(loc, width);
                masks[index] = 2;

                int starCol = FindStarInRow(masks, width, loc.y);
                if (starCol != -1)
                {
                    yCovered[loc.y] = true;
                    xCovered[starCol] = false;
                }
                else
                {
                    pathStart = loc;
                    return 3;
                }
            }
        
        }
        
        private static int RunStep3(NativeArray<byte> masks, NativeArray<bool> yCovered, NativeArray<bool> xCovered, int w, int h, NativeArray<int2> path, int2 pathStart)
        {
            int pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                int row = FindStarInColumn(masks, h, w,path[pathIndex].x);
                if (row == -1) break;
                pathIndex++;
                path[pathIndex] = new int2(row, path[pathIndex - 1].x);

                int col = FindPrimeInRow(masks, w, path[pathIndex].y);
                pathIndex++;
                path[pathIndex] = new int2(path[pathIndex - 1].y, col);
            }
            ConvertPath(masks, path, pathIndex + 1, w);
            ClearCovers(yCovered, xCovered, w, h);
            ClearPrimes(masks, w, h);
            return 1;
        }
        
        private static int RunStep4(NativeArray<int> costs, NativeArray<bool> yCovered, NativeArray<bool> xCovered, int width, int height)
        {
            int minValue = FindMinimum(costs, yCovered, xCovered, width, height);
            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = KzwMath.GetXY(i, width);
                costs[i] += yCovered[y] ? minValue : 0;
                costs[i] -= !xCovered[x] ? minValue : 0;
            }
            return 2;
        }
        
        private static void ConvertPath(NativeArray<byte> masks, NativeArray<int2> path, int pathLength, int width)
        {
            for (int i = 0; i < pathLength; i++)
            {
                int index = KzwMath.GetIndex(path[i], width);
                masks[index] = masks[index] switch
                {
                    1 => 0,
                    2 => 1,
                    _ => masks[index]
                };
            }
        }
        
        private static void ClearPrimes(NativeArray<byte> masks)
        {
            for (int i = 0; i < masks.Length; i++)
            {
                //if (masks[i] == 2) masks[i] = 0;
                masks[i] = masks[i] == 2 ? (byte)0 : masks[i];
            }
        }
        */
        
        
        private static int FindMinimum(NativeArray<int> costs, NativeArray<bool> yCovered, NativeArray<bool> xCovered, int width)
        {
            int minValue = int.MaxValue;
            for (int i = 0; i < costs.Length; i++)
            {
                (int x, int y) = GetXY(i, width);
                minValue = !yCovered[y] && !xCovered[x] ? min(minValue, costs[i]) : minValue;
                //if (!yCovered[y] && !xCovered[x]) minValue = min(minValue, costs[i]);
            }
            return minValue;
        }
        
    }
}
