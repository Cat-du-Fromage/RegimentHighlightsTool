using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace KaizerWald
{
    public class UnitMatrixElement : IComparable<UnitMatrixElement>
    {
        public int IndexInRegiment { get; private set; }
        public RegimentFormationMatrix RegimentFormationMatrix { get; private set; }
        
        public UnitMatrixElement(RegimentFormationMatrix regimentFormationMatrix, int indexInRegiment)
        {
            RegimentFormationMatrix = regimentFormationMatrix;
            IndexInRegiment = indexInRegiment;
        }

        public void SetIndexInRegiment(int index)
        {
            RegimentFormationMatrix.SetIndexIndexInRegiment(IndexInRegiment, index);
        }
        
        public void SetIndexInRegiment(RegimentFormationMatrix regiment, int index)
        {
            IndexInRegiment = index;
        }

        public void SetFormationMatrix(RegimentFormationMatrix formationMatrix) => RegimentFormationMatrix = formationMatrix;
        public int CompareTo(UnitMatrixElement other)
        {
            if (other != null) return this.IndexInRegiment.CompareTo(other.IndexInRegiment);
            Debug.LogError("UnitMatrixElement null Comparer");
            return int.MaxValue;
        }
    }
    
//======================================================================================================================
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ LIMITS ◆◆◆◆◆◆                                                   ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
//======================================================================================================================

    public class RegimentFormationMatrix
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ Properties ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public List<UnitMatrixElement> UnitMatrixElements { get; private set; }
        public List<Unit> Units{ get; private set; }
        public List<Transform> Transforms{ get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public RegimentFormationMatrix(List<Unit> units)
        {
            UnitMatrixElements = new List<UnitMatrixElement>(units.Count);
            Units = new List<Unit>(units.Count);
            Transforms = new List<Transform>(units.Count);
            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                unit.InitializeFormationMatrix(this, i);
                UnitMatrixElements.Add(unit.FormationMatrix);
                Units.Add(unit);
                Transforms.Add(unit.transform);
            }
        }
        
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ METHODS ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public int Count => Units.Count;
        public Unit this[int index] => Units[index];
        
        private void SetIndexInRegiment(int index, int value)
        {
            UnitMatrixElements[index].SetIndexInRegiment(this, value);
        }

        public void SwapIndexInRegiment(int currentIndex ,int indexToSwapWith)
        {
            UnitMatrixElements[currentIndex].SetIndexInRegiment(this, indexToSwapWith);
            UnitMatrixElements[indexToSwapWith].SetIndexInRegiment(this, currentIndex);
            
            UnitMatrixElements.Swap(currentIndex, indexToSwapWith);
            Units.Swap(currentIndex, indexToSwapWith);
            Transforms.Swap(currentIndex, indexToSwapWith);
        }

        public void SetIndexIndexInRegiment(int currentIndex, int indexToSwapWith)
        {
            SwapIndexInRegiment(currentIndex, indexToSwapWith);
        }

        public void SetAllIndices(int[] indices)
        {
            if (indices.Length != Units.Count) return;
            for (int i = 0; i < indices.Length; i++)
            {
                SetIndexInRegiment(i, indices[i]);
            }
            Units.Sort();
            
            for (int i = 0; i < Units.Count; i++)
            {
                UnitMatrixElements[i] = Units[i].FormationMatrix;
                Transforms[i] = Units[i].transform;
            }
        }
        
        public void Add(Unit unit)
        {
            unit.InitializeFormationMatrix(this, Units.Count);
            UnitMatrixElements.Add(unit.FormationMatrix);
            Units.Add(unit);
            Transforms.Add(unit.transform);
        }
        
        /// <summary>
        /// Remove element at Index
        /// Buffers are reorganize before removale
        /// </summary>
        public void Resize(int numToRemove)
        {
            int startIndex = Units.Count - numToRemove;
            UnitMatrixElements.RemoveRange(startIndex, numToRemove);
            Transforms.RemoveRange(startIndex, numToRemove);
            Units.RemoveRange(startIndex, numToRemove);
        }
        
        /// <summary>
        /// Remove element at Index
        /// Buffers are reorganize before removale
        /// </summary>
        public void Remove(Unit unit)
        {
            RemoveAt(unit.IndexInRegiment);
        }
        
        /// <summary>
        /// Remove element at Index
        /// Buffers are reorganize before removale
        /// </summary>
        public void RemoveAt(int indexInRegiment)
        {
            Reorganize(indexInRegiment);
            int lastIndex = Units.Count - 1;
            UnitMatrixElements.RemoveAt(lastIndex);
            Units.RemoveAt(lastIndex);
            Transforms.RemoveAt(lastIndex);
        }

        /// <summary>
        /// Reorganize alle the buffer so Index in regiment correspond to Index in Buffer
        /// Units to be remove is placed at the end for a safe remove
        /// </summary>
        /// <param name="indexInRegiment">Index in buffer to remove</param>
        private void Reorganize(int indexInRegiment)
        {
            //if (indexInRegiment == Units.Count - 1) return;
            int numIteration = Units.Count - 1;
            for (int i = indexInRegiment; i < numIteration; i++)
            {
                SwapIndexInRegiment(i, i + 1);
            }
        }
    }
}