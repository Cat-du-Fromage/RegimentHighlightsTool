using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Mathematics.math;
using static Kaizerwald.KzwMath;

namespace Kaizerwald
{
    public static class JobifiedHungarianAlgorithm
    {
        public static NativeArray<int> FindAssignments(this NativeArray<int> costs, int width)
        {
            int height = costs.Length / width;
            
            //Find And Subtract minimum value from each element in the row
            NativeArray<int> minValues = new (height, TempJob);
            JobHandle minDependency = costs.FindRowsMinimums(minValues, width);
            JobHandle subtractMinValuesDependency = JSubtractMinValue.Process(costs, minValues, width, minDependency);
            minValues.Dispose(subtractMinValuesDependency);
            
            NativeArray<byte> masks = new (costs.Length, TempJob, ClearMemory);
            NativeArray<bool> yCovered = new (height, TempJob, ClearMemory);
            NativeArray<bool> xCovered = new (width, TempJob, ClearMemory);
            
            // Methods: CreateMasksAndCoveredArrays
            JobHandle zeroCoveredDependency = JCreateMasksAndCoveredArrays.Process(costs, masks, yCovered, xCovered, width, subtractMinValuesDependency);
            JobHandle clearCoveredDependency = JClearCovers.Process(width, height, yCovered, xCovered, zeroCoveredDependency);
            
            NativeArray<int2> path = new (costs.Length, TempJob, ClearMemory);
            (int2 pathStart, int step) = (default, 1);

            JobHandle currentDependency = clearCoveredDependency;
            while (step != -1)
            {
                step = step switch
                {
                    1 => RunStep1(width, height, ref masks, ref xCovered, ref currentDependency),
                    2 => RunStep2(width, ref costs, ref masks, ref xCovered, ref yCovered, ref pathStart, currentDependency),
                    //3 => RunStep3(),
                    //4 => RunStep4(),
                    _ => step
                };
            }

            masks.Dispose();
            yCovered.Dispose();
            xCovered.Dispose();
            path.Dispose();
            return default;
        }
        
        // =============================================================================================================
        //Find And Subtract minimum value from each element in the row
        public static JobHandle FindRowsMinimums(this NativeArray<int> costs, NativeArray<int> minValues, int width, JobHandle dependency = default)
        {
            int height = costs.Length / width;
            NativeArray<JobHandle> minJobHandles = new (height, Temp, UninitializedMemory);
            for (int y = 0; y < height; y++)
            {
                int minIndex = y * width;
                JFindMinValue job = new JFindMinValue
                {
                    Index = y,
                    Row = costs.Slice(minIndex, width),
                    Mins = minValues
                };
                minJobHandles[y] = job.Schedule(dependency);
            }
            JobHandle minDependency = JobHandle.CombineDependencies(minJobHandles);
            return minDependency;
        }
        // =============================================================================================================
        
        private static int RunStep1(int width, int height, ref NativeArray<byte> masks, ref NativeArray<bool> xCovered, ref JobHandle dependency)
        {
            JobHandle columnsCoveredDependency = JRunStep1GetColumnsCovered.Process(masks, xCovered, width, dependency);
            using NativeReference<int> colsCoveredCount = new (0, TempJob);
            JobHandle jobHandle = JRunStep1CountColumnsCovered.Process(colsCoveredCount, xCovered, width, columnsCoveredDependency);
            jobHandle.Complete();
            dependency = default;
            return colsCoveredCount.Value == height ? -1 : 2;
        }
        
        private static int RunStep2(int width, ref NativeArray<int> costs, ref NativeArray<byte> masks, ref NativeArray<bool> xCovered, ref NativeArray<bool> yCovered,  ref int2 pathStart, JobHandle dependency = default)
        {
#if UNITY_EDITOR
            const int SAFE_GUARD = 1000;
            int safeIterator = 0;
#endif 
            while (true)
            {
#if UNITY_EDITOR
                if (safeIterator >= SAFE_GUARD) return -1;
                safeIterator++;
#endif
                int2 zeroCoords = JFindZeros.ProcessComplete(width, costs, yCovered, xCovered, dependency);
                
                if (zeroCoords.y == -1) return 4;
                masks[GetIndex(zeroCoords, width)] = 2;

                int starCoordX = JFindStarInRow.ProcessComplete(width, zeroCoords.y, masks, dependency);
                if (starCoordX != -1)
                {
                    yCovered[zeroCoords.y] = true;
                    xCovered[starCoordX] = false;
                }
                else
                {
                    pathStart = zeroCoords;
                    return 3;
                }
            }
        }
        //TODO REPRENDRE ICI!!!!!!!!!!!!
        /*
        private static int RunStep3(int w, int h, ref NativeArray<byte> masks, ref NativeArray<bool> yCovered, ref NativeArray<bool> xCovered, ref NativeArray<int2> path, int2 pathStart, JobHandle dependency = default)
        {
#if UNITY_EDITOR
            const int SAFE_GUARD = 1000;
            int safeIterator = 0;
#endif 
            int pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
#if UNITY_EDITOR
                if (safeIterator >= SAFE_GUARD) return -1;
                safeIterator++;
#endif
                int row = JFindStarInColumn.ProcessComplete(w, h, path[pathIndex].x, ref masks, dependency);
                if (row == -1) break;
                pathIndex++;
                path[pathIndex] = new int2(row, path[pathIndex - 1].x);

                int col = FindPrimeInRow(masks, w, path[pathIndex].y);
                
                pathIndex++;
                path[pathIndex] = new int2(path[pathIndex - 1].y, col);
            }
            ConvertPath(masks, path, pathIndex + 1, w);
            //ClearCovers(yCovered, xCovered, w, h);
            JobHandle clearCoveredDependency = JClearCovers.Process(width, height, yCovered, xCovered, zeroCoveredDependency);
            ClearPrimes(masks, w, h);
            return 1;
        }
        
        private static int FindPrimeInRow(NativeArray<byte> masks, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                int index = GetIndex(x,y,width);
                if (masks[index] == 2) return x;
            }
            return -1;
        }
        */
    }
    
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ JOBS ◆◆◆◆◆◆                                                   ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    #region Step1
    // =============================================================================================================
    // STEP 1
    public struct JRunStep1CountColumnsCovered : IJob
    {
        [ReadOnly] public int Width;
        [ReadOnly, NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
        public NativeArray<bool> XCovered;
        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeReference<int> ColsCoveredCount;
        
        public void Execute()
        {
            int colsCoveredCount = 0;
            for (int x = 0; x < Width; x++)
            {
                colsCoveredCount += select(0, 1, XCovered[x]);
            }
            ColsCoveredCount.Value = colsCoveredCount;
        }

        public static JobHandle Process(NativeReference<int> colsCoveredCount, NativeArray<bool> xCovered, int width, JobHandle dependency = default)
        {
            JRunStep1CountColumnsCovered job = new JRunStep1CountColumnsCovered
            {
                Width = width,
                XCovered = xCovered,
                ColsCoveredCount = colsCoveredCount
            };
            JobHandle jobHandle = job.Schedule(dependency);
            return jobHandle;
        }
    }
    
    public struct JRunStep1GetColumnsCovered : IJobFor
    {
        [ReadOnly] public int Width;
        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<byte> Masks;
        [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
        public NativeArray<bool> XCovered;
        
        public void Execute(int index)
        {
            int x = index % Width;
            //XCovered[x] |= Masks[index] == 1;
            if (Masks[index] != 1) return;
            XCovered[x] = true;
        }

        public static JobHandle Process(NativeArray<byte> masks, NativeArray<bool> xCovered, int width, JobHandle dependency = default)
        {
            JRunStep1GetColumnsCovered job = new JRunStep1GetColumnsCovered
            {
                Width = width,
                Masks = masks,
                XCovered = xCovered
            };
            JobHandle jobHandle = job.ScheduleParallel(masks.Length, JobsUtility.JobWorkerCount - 1, dependency);
            return jobHandle;
        }
    }
    // =============================================================================================================
    #endregion

    #region Step2

    //TODO CAN DO BETTER!
    public struct JFindStarInRow : IJob
    {
        [ReadOnly] public int Width;
        [ReadOnly] public int ZeroCoordY;
        
        [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<byte> Masks;
        
        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public NativeReference<int> CoordX;
        
        public void Execute()
        {
            for (int x = 0; x < Width; x++)
            {
                int index = KzwMath.GetIndex(x, ZeroCoordY, Width);
                if (Masks[index] != 1) continue;
                CoordX.Value = x;
                return;
            }
            CoordX.Value = -1;
        }
        
        public static JobHandle Process(NativeReference<int> coordX, int width, int zeroCoordY,NativeArray<byte> masks, JobHandle dependency = default)
        {
            JFindStarInRow job = new JFindStarInRow
            {
                Width = width,
                ZeroCoordY = zeroCoordY,
                Masks = masks,
                CoordX = coordX,
            };
            JobHandle jobHandle = job.Schedule(dependency);
            return jobHandle;
        }
        
        public static int ProcessComplete(int width, int zeroCoordY, NativeArray<byte> masks, JobHandle dependency = default)
        {
            using NativeReference<int> coordX = new (-1, TempJob);
            JobHandle jobHandle = Process(coordX, width, zeroCoordY, masks, dependency);
            jobHandle.Complete();
            return coordX.Value;
        }
    }

    public struct JFindZeros : IJob
    {
        [ReadOnly] public int Width;
        
        [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<int> Costs;
        [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<bool> YCovered;
        [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<bool> XCovered;

        [WriteOnly, NativeDisableContainerSafetyRestriction]
        private NativeReference<int2> ZeroCoord;
        
        public void Execute()
        {
            for (int i = 0; i < Costs.Length; i++)
            {
                (int x, int y) = KzwMath.GetXY(i, Width);
                if (Costs[i] != 0 || YCovered[y] || XCovered[x]) continue;
                ZeroCoord.Value = new int2(x, y);
                return;
            }
            ZeroCoord.Value = new int2(-1, -1);
        }

        public static JobHandle Process(NativeReference<int2> zeroCoord, int width, NativeArray<int> costs,
            NativeArray<bool> yCovered, NativeArray<bool> xCovered, JobHandle dependency = default)
        {
            JFindZeros job = new JFindZeros
            {
                Width = width,
                Costs = costs,
                YCovered = yCovered,
                XCovered = xCovered,
                ZeroCoord = zeroCoord
            };
            JobHandle jobHandle = job.Schedule(dependency);
            return jobHandle;
        }
        
        public static int2 ProcessComplete(int width, NativeArray<int> costs,
            NativeArray<bool> yCovered, NativeArray<bool> xCovered, JobHandle dependency = default)
        {
            using NativeReference<int2> zeroCoord = new (int2(-1), TempJob);
            JobHandle jobHandle = Process(zeroCoord, width, costs, yCovered, xCovered, dependency);
            jobHandle.Complete();
            return zeroCoord.Value;
        }
    }

    #endregion
    //TODO REPRENDRE ICI!!!!!!!!!!!!
    #region Step3
/*
    public struct JFindPrimeInRow : IJob
    {
        [ReadOnly] public int Width;
        [ReadOnly] private int ZeroCoordY;
        
        [ReadOnly, NativeDisableContainerSafetyRestriction]
        public NativeArray<byte> Masks;
        
        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public NativeReference<int> CoordY;
        
        public void Execute()
        {
            for (int x = 0; x < Width; x++)
            {
                int index = GetIndex(x,ZeroCoordY,Width);
                if (masks[index] == 2) return x;
            }
            return -1;
        }
    }
    */
    public struct JFindStarInColumn : IJob
    {
        [ReadOnly] public int Width;
        [ReadOnly] public int Height;
        [ReadOnly] public int ZeroCoordX;
        
        [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<byte> Masks;
            
        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public NativeReference<int> CoordY;
            
        public void Execute()
        {
            for (int y = 0; y < Height; y++)
            {
                int index = GetIndex(ZeroCoordX, y, Width);
                if (Masks[index] != 1) continue;
                CoordY.Value = y;
                return;
            }
            CoordY.Value = -1;
        }
            
        public static JobHandle Process(NativeReference<int> coordY, int width, int height, int zeroCoordX, ref NativeArray<byte> masks, JobHandle dependency = default)
        {
            JFindStarInColumn job = new JFindStarInColumn
            {
                Width = width,
                Height = height,
                ZeroCoordX = zeroCoordX,
                Masks = masks,
                CoordY = coordY,
            };
            JobHandle jobHandle = job.Schedule(dependency);
            return jobHandle;
        }
            
        public static int ProcessComplete(int width, int height, int zeroCoordX, ref NativeArray<byte> masks, JobHandle dependency = default)
        {
            using NativeReference<int> coordY = new (-1, TempJob);
            JobHandle jobHandle = Process(coordY, width,height, zeroCoordX, ref masks, dependency);
            jobHandle.Complete();
            return coordY.Value;
        }
    }

    #endregion
    
    public struct JFindMinValue : IJob
    {
        [ReadOnly] public int Index;
        
        [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeSlice<int> Row;
        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<int> Mins;
        
        public void Execute()
        {
            int minValue = int.MaxValue;
            for (int i = 0; i < Row.Length; i++)
            {
                minValue = min(minValue, Row[i]);
            }
            Mins[Index] = minValue;
        }
    }
    
    public struct JSubtractMinValue : IJobFor
    {
        [ReadOnly] public int Width;
        [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<int> Mins;
        [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
        public NativeArray<int> Costs;
        
        public void Execute(int index)
        {
            int y = index / Width;
            Costs[index] -= Mins[y];
        }
        
        public static JobHandle Process(NativeArray<int> costs, NativeArray<int> minValues, int width, JobHandle minDependency = default)
        {
            JSubtractMinValue job = new JSubtractMinValue
            {
                Width = width,
                Mins = minValues,
                Costs = costs
            };
            JobHandle jobHandle = job.ScheduleParallel(costs.Length, JobsUtility.JobWorkerCount - 1, minDependency);
            return jobHandle;
        }
    }
    
    public struct JCreateMasksAndCoveredArrays : IJobFor
    {
        [ReadOnly] public int Width;
        
        [ReadOnly, NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
        public NativeArray<int> Costs;
        
        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
        public NativeArray<byte> Masks;
        [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
        public NativeArray<bool> YCovered;
        [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
        public NativeArray<bool> XCovered;
        
        public void Execute(int index)
        {
            (int x, int y) = GetXY(index, Width);
            if (Costs[index] != 0 || YCovered[y] || XCovered[x]) return;
            Masks[index] = 1;
            YCovered[y] = true;
            XCovered[x] = true;
        }
        
        public static JobHandle Process(NativeArray<int> costs, NativeArray<byte> masks, NativeArray<bool> yCovered, NativeArray<bool> xCovered, int width, JobHandle dependency = default)
        {
            JCreateMasksAndCoveredArrays job = new JCreateMasksAndCoveredArrays
            {
                Width = width,
                Costs = costs,
                Masks = masks,
                YCovered = yCovered,
                XCovered = xCovered
            };
            JobHandle jobHandle = job.ScheduleParallel(costs.Length, JobsUtility.JobWorkerCount - 1, dependency);
            return jobHandle;
        }
    }

    public struct JClearCovers : IJob
    {
        [ReadOnly] public int2 WidthHeight;
        [WriteOnly, NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
        public NativeArray<bool> YCovered;
        [WriteOnly, NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
        public NativeArray<bool> XCovered;
        
        public void Execute()
        {
            for (int y = 0; y < WidthHeight[1]; y++) { YCovered[y] = false; }
            for (int x = 0; x < WidthHeight[0]; x++) { XCovered[x] = false; }
        }

        public static JobHandle Process(int width, int height, NativeArray<bool> yCovered, NativeArray<bool> xCovered, JobHandle dependency = default)
        {
            JClearCovers job = new JClearCovers
            {
                WidthHeight = int2(width, height),
                YCovered = yCovered,
                XCovered = xCovered
            };
            JobHandle jobHandle = job.Schedule(dependency);
            return jobHandle;
        }
    }
}
