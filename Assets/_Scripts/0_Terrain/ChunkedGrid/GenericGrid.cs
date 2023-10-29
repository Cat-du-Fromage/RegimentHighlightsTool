
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWald.KzwMath;
using static KaizerWald.UnityMathematicsUtilities;

using float2 = Unity.Mathematics.float2; 

namespace KWUtils
{
    public class GenericGrid<T> : IDisposable
    where T : struct
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected readonly bool IsCentered;
        protected readonly int CellSize;
        protected readonly int2 MapSizeXY;
        protected readonly int2 NumCellXY;

        public NativeArray<T> GridArray;
        public event Action OnGridChange;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public GenericGrid(in int2 mapSize, int cellSize, Func<int, T> createGridObject, bool isCentered = true)
        {
            IsCentered = isCentered;
            CellSize = max(1,cellSize);
            MapSizeXY = ceilpow2(mapSize);
            NumCellXY = mapSize >> floorlog2(CellSize);
            GridArray = new NativeArray<T>(NumCellXY.x * NumCellXY.y, Allocator.Persistent);
            
            //Init Grid
            //Example: new GenericGrid(int2(8,8), 2, (i) => i)
            //Will populate the grid like so: {0, 1, 2, 3....}
            //Example: new GenericGrid(int2(8,8), 2, (i.GetXY) => int2(x,y))
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(i);
            }
        }
        
        public GenericGrid(in int2 mapSize, int cellSize, bool isCentered = true)
        {
            IsCentered = isCentered;
            CellSize = max(1,cellSize);
            MapSizeXY = ceilpow2(mapSize);
            NumCellXY = mapSize >> floorlog2(CellSize);
            GridArray = new NativeArray<T>(NumCellXY.x * NumCellXY.y, Allocator.Persistent);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //Clear Events
        public virtual void ClearEvents()
        {
            if (OnGridChange == null) return;
            Array.ForEach(OnGridChange.GetInvocationList(), action => OnGridChange -= (Action)action);
            //foreach (Delegate action in OnGridChange.GetInvocationList()) { OnGridChange -= (Action)action; }
        }

        //==============================================================================================================
        //CELLS INFORMATION
        //==============================================================================================================

        public float3 GetCellCenter(int index)
        {
            float2 offset = IsCentered ? (float2)MapSizeXY / 2f : float2.zero;
            float2 cellCoord = GetXY2(index,NumCellXY.x) * CellSize + float2(CellSize/2f) - offset;
            return new float3(cellCoord.x,0,cellCoord.y);
        }
        
        //==============================================================================================================
        //ARRAY MANIPULATION
        //==============================================================================================================
        
        public virtual void CopyFrom(T[] otherArray)
        {
            otherArray.CopyTo(GridArray.AsSpan());
        }
        
        public virtual void CopyFrom(NativeArray<T> otherArray)
        {
            otherArray.CopyTo(GridArray);
        }
        
        public T this[int cellIndex]
        {
            get => GridArray[cellIndex];
            set => SetValue(cellIndex, value);
        }
        
        public T GetValue(int index)
        {
            return GridArray[index];
        }

        public virtual void SetValue(int index, T value)
        {
            GridArray[index] = value;
            OnGridChange?.Invoke();
        }
        
        //Operation from World Position
        //==============================================================================================================
        public int IndexFromPosition(in float3 position)
        {
            return IsCentered ? GetIndexFromPositionOffset(position.xz,MapSizeXY,CellSize) : GetIndexFromPosition(position.xz,MapSizeXY,CellSize);
        }
        
        private int GetIndexFromPosition(in float2 pointPos, in int2 mapXY, int cellSize = 1)
        {
            float2 percents = saturate(pointPos / (mapXY * cellSize));
            int2 coord =  clamp((int2)floor(mapXY * percents), 0, mapXY - 1);
            return GetIndex(coord, mapXY.x / cellSize);
        }
        
        private int GetIndexFromPositionOffset(in float2 pointPos, in int2 mapXY, int cellSize = 1)
        {
            float2 offset = (float2)mapXY / 2f;
            float2 percents = saturate((pointPos + offset) / (mapXY * cellSize));
            int2 coord =  clamp((int2)floor(mapXY * percents), 0, mapXY - 1); // Cellsize not applied?!
            return GetIndex(coord, mapXY.x / cellSize);
        }

        public void Dispose()
        {
            ClearEvents();
            GridArray.Dispose();
        }
    }
}
