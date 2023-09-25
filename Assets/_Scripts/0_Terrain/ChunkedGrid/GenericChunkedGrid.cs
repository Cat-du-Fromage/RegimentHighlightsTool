/*
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWald.KzwMath;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;

using float2 = Unity.Mathematics.float2; 

namespace KWUtils
{
    public sealed class GenericChunkedGrid<T> : GenericGrid<T>
    where T : struct
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private readonly int ChunkSize;
        private readonly int2 NumChunkXY;
        public new event Action OnGridChange;
        public Dictionary<int, T[]> ChunkDictionary { get; private set; }
        public sealed override GridData GridData => new GridData(MapXY, CellSize, ChunkSize);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public GenericChunkedGrid(in int2 mapSize, int chunkSize, int cellSize, Func<int, T> createGridObject, bool centered = false) : base(in mapSize, cellSize, createGridObject, centered)
        {
            this.ChunkSize = GetChunkSize(chunkSize, cellSize);
            NumChunkXY = mapSize >> floorlog2(chunkSize);

            ChunkDictionary = new Dictionary<int, T[]>(NumChunkXY.x * NumChunkXY.y);
            ChunkDictionary = GetGridValueOrderedByChunk(GridArray, GridData);
        }
        
        public GenericChunkedGrid(in int2 mapSize, int chunkSize, int cellSize = 1, bool centered = false, [CanBeNull] Func<T[]> providerFunction = null) : base(in mapSize, cellSize, centered)
        {
            this.ChunkSize = GetChunkSize(chunkSize, cellSize);
            NumChunkXY = mapSize >> floorlog2(chunkSize);

            providerFunction?.Invoke()?.CopyTo((Span<T>) GridArray); //CAREFULL may switch with Memory<T>!
            ChunkDictionary = new Dictionary<int, T[]>(NumChunkXY.x * NumChunkXY.y);
            ChunkDictionary = GetGridValueOrderedByChunk(GridArray, GridData);
        }
        
        /// Make sur ChunkSize is Greater than cellSize
        private int GetChunkSize(int chunksSize ,int cellSize)
        {
            int value = ceilpow2(chunksSize);
            while (value <= cellSize) { value *= 2; }
            return value;
        }
        //==============================================================================================================

        //Clear Events
        public sealed override void ClearEvents()
        {
            if (OnGridChange == null) return;
            foreach (Delegate action in OnGridChange.GetInvocationList())
            {
                OnGridChange -= (Action)action;
            }
        }

        //==============================================================================================================
        //Cell Data
        //==========
        public Vector3 GetChunkCenter(int chunkIndex)
        {
            float2 offset = IsCentered ? ((float2)NumChunkXY * ChunkSize / 2) : float2.zero;
            float2 chunkCoord = GetXY2(chunkIndex,NumChunkXY.x) * ChunkSize + new float2(ChunkSize/2f) - offset;
            return new Vector3(chunkCoord.x, 0, chunkCoord.y);
        }
        public Vector3 GetChunkCellCenter(int chunkIndex, int cellIndexInChunk)
        {
            int indexInGrid = GetGridCellIndexFromChunkCellIndex(chunkIndex, GridData, cellIndexInChunk);
            return GetCellCenter(indexInGrid);
        }

        public Vector3[] GetChunkCellsCenter(int chunkIndex)
        {
            int totalCells = NumCellXY.x * NumCellXY.y;
            Vector3[] centers = new Vector3[totalCells];
            for (int i = 0; i < totalCells; i++)
            {
                centers[i] = GetChunkCellCenter(chunkIndex, i);
            }
            return centers;
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Connection between chunk and Grid
        //==================================
        public int ChunkIndexFromGridIndex(int gridIndex)
        {
            int2 cellCoord = GetXY2(gridIndex,MapXY.x);
            int2 chunkCoord = (int2)floor(cellCoord / ChunkSize);
            return GetIndex(chunkCoord, NumChunkXY.x);
        }
        
        public int CellChunkIndexFromGridIndex(int gridIndex)
        {
            int2 cellCoord = GetXY2(gridIndex,MapXY.x);
            int2 chunkCoord = (int2)floor(cellCoord / ChunkSize);
            int2 cellCoordInChunk = cellCoord - (chunkCoord * ChunkSize);
            return GetIndex(cellCoordInChunk,ChunkSize);
        }
        
        public int GetChunkIndexFromPosition(float3 pos)
        {
            float2 offset = IsCentered ?GridData.NumCellXY / new float2(2f) : float2.zero;
            float2 percents = (pos.xz + offset) / (NumChunkXY * ChunkSize);
            percents = clamp(percents, float2.zero, 1f);
            int2 xy =  clamp((int2)floor(NumChunkXY * percents), 0, NumChunkXY - 1); // Cellsize not applied?!
            return mad(xy.y, NumChunkXY.x, xy.x);
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Set both grid value and Chunk Value
        //====================================
        public sealed override void CopyFrom(T[] otherArray)
        {
            base.CopyFrom(otherArray);
            PopulateChunkedGrid(ChunkDictionary, GridArray, GridData);
        }
        
        public sealed override void CopyFrom(NativeArray<T> otherArray)
        {
            base.CopyFrom(otherArray);
            PopulateChunkedGrid(ChunkDictionary, GridArray, GridData);
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Update Chunk or Array according to the type of value set
        //=========================================================
        
        /// Chunk : Update made after the Array was modified
        private void UpdateChunk(int gridIndex, T value)
        {
            int2 cellCoord = GetXY2(gridIndex,MapXY.x);
            //Chunk Index
            int2 chunkCoord = (int2)floor(cellCoord / ChunkSize);
            int chunkIndex = GetIndex(chunkCoord,NumChunkXY.x);
            //CellIndex
            int2 cellCoordInChunk = cellCoord - (chunkCoord * ChunkSize);
            int cellIndexInChunk = GetIndex(cellCoordInChunk,ChunkSize);
            
            ChunkDictionary[chunkIndex][cellIndexInChunk] = value;
        }
        
        /// Array : Update made after a Chunk was modified
        private void UpdateGrid(int chunkIndex, T[] values)
        {
            for (int i = 0; i < values.Length; i++)
                GridArray[GetGridCellIndexFromChunkCellIndex(chunkIndex, GridData, i)] = values[i];
        }
        
        private void UpdateGrid(int chunkIndex, NativeSlice<T> values)
        {
            for (int i = 0; i < values.Length; i++)
                GridArray[GetGridCellIndexFromChunkCellIndex(chunkIndex, GridData, i)] = values[i];
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Set Values inside a chunk
        public sealed override void SetValue(int index, T value)
        {
            GridArray[index] = value;
            UpdateChunk(index, value);
            OnGridChange?.Invoke();
        }
        public void SetValues(int chunkIndex, T[] values)
        {
            values.CopyTo((Span<T>)ChunkDictionary[chunkIndex]);
            UpdateGrid(chunkIndex, values);
            OnGridChange?.Invoke();
        }
        
        public void SetValues(int chunkIndex, NativeSlice<T> values)
        {
            values.CopyTo(ChunkDictionary[chunkIndex]);
            UpdateGrid(chunkIndex, values);
            OnGridChange?.Invoke();
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Get Values inside a chunk
        public T[] GetValues(int index) => ChunkDictionary[index];
        public T[] GetValues(int x, int y) => ChunkDictionary[y * NumCellXY.x + x];
        public T[] GetValues(int2 coord) => ChunkDictionary[coord.y * NumCellXY.x + coord.x];
        //==============================================================================================================
        
        //==============================================================================================================
        //Get/Set Value By Index
        public new T[] this[int chunkIndex]
        {
            get => ChunkDictionary[chunkIndex];
            set => SetValues(chunkIndex, value);
        }
        //==============================================================================================================
    }
}
*/