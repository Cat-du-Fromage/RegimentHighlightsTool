/*
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;

using float2 = Unity.Mathematics.float2; 

namespace KWUtils
{
    public class GenericGrid<T>
    where T : struct
    {
        protected readonly bool IsCentered;
        protected readonly int CellSize;
        protected readonly int2 MapXY;
        protected readonly int2 NumCellXY;

        public readonly T[] GridArray;
        public event Action OnGridChange;
        
        //==============================================================================================================
        //CONSTRUCTOR
        //==============================================================================================================
        public GenericGrid(in int2 mapSize, int cellSize, Func<int, T> createGridObject, bool isCentered = false)
        {
            IsCentered = isCentered;
            CellSize = cellSize;
            MapXY = ceilpow2(mapSize);
            NumCellXY = mapSize >> floorlog2(cellSize);
            GridArray = new T[NumCellXY.x * NumCellXY.y];
            
            //Init Grid
            //Example: new GenericGrid(int2(8,8), 2, (i) => i)
            //Will populate the grid like so: {0, 1, 2, 3....}
            //Example: new GenericGrid(int2(8,8), 2, (i.GetXY) => int2(x,y))
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(i);
            }
        }
        
        public GenericGrid(in int2 mapSize, int cellSize, bool isCentered = false)
        {
            IsCentered = isCentered;
            CellSize = cellSize;
            MapXY = ceilpow2(mapSize);
            NumCellXY = mapSize >> floorlog2(cellSize);
            GridArray = new T[NumCellXY.x * NumCellXY.y];
        }
        
        public virtual GridData GridData => new GridData(MapXY, CellSize);
        
        //Clear Events
        public virtual void ClearEvents()
        {
            if (OnGridChange == null) return;
            //Array.ForEach(OnGridChange.GetInvocationList(), action => OnGridChange -= (Action)action);
            foreach (Delegate action in OnGridChange.GetInvocationList())
            {
                OnGridChange -= (Action)action;
            }
        }

        //==============================================================================================================
        //CELLS INFORMATION
        //==============================================================================================================

        public Vector3 GetCellCenter(int index)
        {
            float2 offset = IsCentered ? ((float2)MapXY / 2) : float2.zero;
            float2 cellCoord = GetXY2(index,NumCellXY.x) * CellSize + float2(CellSize/2f) - offset;
            return new Vector3(cellCoord.x,0,cellCoord.y);
        }
        
        //==============================================================================================================
        //ARRAY MANIPULATION
        //==============================================================================================================
        
        public virtual void CopyFrom(T[] otherArray)
        {
            otherArray.CopyTo((Span<T>) GridArray);
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
        
        //TODO : ADD TO DLL
        public virtual void SetValueFromGreaterGrid(int bigGridCellIndex, int otherCellSize, T value)
        {
            GridData fakeChunk = new GridData(MapXY, CellSize, otherCellSize);
            for (int i = 0; i < fakeChunk.TotalCellInChunk; i++)
            {
                int index = GetGridCellIndexFromChunkCellIndex(bigGridCellIndex, fakeChunk, i);
                GridArray[index] = value;
            }
            OnGridChange?.Invoke();
        }
        
        //Operation from World Position
        //==============================================================================================================
        public int IndexFromPosition(in Vector3 position)
        {
            return IsCentered 
                ? GetIndexFromPositionOffset(position.XZ(),MapXY, CellSize) 
                : GetIndexFromPosition(position.XZ(),MapXY, CellSize);
        }

        //==============================================================================================================
        //Adaptation to an other Grid with different Cell
        //==============================================================================================================

        //AdaptGrid(GenericGrid<T> grid)
        /// <summary>
        /// CAREFULL WITH FAKE CHUNK! CHUNKSIZE == 1!!!
        /// </summary>
        
        public T[] AdaptGrid<T1>(GenericGrid<T1> otherGrid)
        where T1 : struct
        {
            if (CellSize < otherGrid.CellSize)
            {
                //We Receive Grid With bigger Cells!
                //TO MUCH REFLECTION! if V3 => moyenne des vecteurs? si int moyenne de valeur?
                //GridData fakeChunk = new GridData(MapXY, CellSize, otherGrid.CellSize);
                //return GridArray.AdaptBigToSmallGrid(GridData, fakeChunk);
                return GridArray;
            }
            else if (CellSize > otherGrid.CellSize)
            {
                //We Receive Grid With smaller Cells!
                GridData fakeChunk = new GridData(MapXY, otherGrid.CellSize, CellSize);
                return GridArray.AdaptToSmallerGrid(otherGrid.GridData, fakeChunk);
            }
            else
            {
                return GridArray;
            }
        }
        
        public NativeArray<T> NativeAdaptGrid<T1>(GenericGrid<T1> otherGrid)
            where T1 : struct
        {
            if (CellSize < otherGrid.CellSize)
            {
                //We Receive Grid With bigger Cells!
                //TO MUCH REFLECTION! if V3 => moyenne des vecteurs? si int moyenne de valeur?
                //GridData fakeChunk = new GridData(MapXY, CellSize, otherGrid.CellSize);
                //return GridArray.AdaptBigToSmallGrid(GridData, fakeChunk);
                return GridArray.ToNativeArray();
            }
            else if (CellSize > otherGrid.CellSize)
            {
                //We Receive Grid With smaller Cells!
                GridData fakeChunk = new GridData(MapXY, otherGrid.CellSize, CellSize);
                return GridArray.NativeAdaptToSmallerGrid(otherGrid.GridData, fakeChunk);
            }
            else
            {
                return GridArray.ToNativeArray();
            }
        }
    }
}
*/
