using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    [System.Serializable]
    public struct FormationData
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        // IMMUTABLES FIELDS
        public readonly ushort BaseNumUnits;
        private readonly byte minRow; 
        private readonly byte maxRow;
        private readonly half2 unitSize;
        private readonly half spaceBetweenUnits;
        
        //MUTABLES FIELDS
        [SerializeField] private ushort numUnitsAlive;
        [SerializeField] private byte width;
        [SerializeField] private byte depth;
        [SerializeField] private half2 direction2DForward;
      //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
      //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
      //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public readonly float SpaceBetweenUnits => spaceBetweenUnits;
        public readonly float2 UnitSize => unitSize;
        public readonly int NumUnitsAlive => numUnitsAlive;
        
        public readonly int Width => width;
        public readonly int Depth => depth;
        public readonly int2 WidthDepth => new int2(width, depth);
        
        public readonly int MinRow => min((int)minRow, numUnitsAlive);
        public readonly int MaxRow => min((int)maxRow, numUnitsAlive);
        public readonly int2 MinMaxRow => new int2(MinRow, MaxRow);
        
        private readonly half2 Direction2DForward => direction2DForward;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public FormationData(RegimentType regimentType, Vector3 direction)
        {
            RegimentClass regimentClass = regimentType.RegimentClass;
            BaseNumUnits = numUnitsAlive = (ushort)regimentClass.BaseNumberUnit;
            minRow = (byte)min(byte.MaxValue, regimentClass.MinRow);
            maxRow = (byte)min(byte.MaxValue,regimentClass.MaxRow);
            
            unitSize = (half2)float2(regimentClass.Category.UnitSize.x, regimentClass.Category.UnitSize.z);
            spaceBetweenUnits = (half)regimentClass.SpaceBetweenUnits;
            
            width = maxRow;
            depth = (byte)(numUnitsAlive / width);
            
            direction2DForward = half2(normalizesafe(direction).xz);
        }
        
        public FormationData(in FormationData otherFormation)
        {
            BaseNumUnits = numUnitsAlive = otherFormation.BaseNumUnits;
            minRow = otherFormation.minRow;
            maxRow = otherFormation.maxRow;
            unitSize = otherFormation.unitSize;
            spaceBetweenUnits = otherFormation.spaceBetweenUnits;
            
            width = otherFormation.maxRow;
            depth = otherFormation.minRow;
            
            direction2DForward = otherFormation.Direction2DForward;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ METHODS ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public readonly float2 DistanceUnitToUnit => UnitSize + SpaceBetweenUnits;
        
        //(Needed for Rearangemment)
        public readonly int NumCompleteLine => Depth * Width == numUnitsAlive ? depth : depth - 1;
        private readonly int CountUnitsLastLine => numUnitsAlive - NumCompleteLine * width;
        public readonly int NumUnitsLastLine => CountUnitsLastLine is 0 ? width : CountUnitsLastLine;

        public void Remove(int numRemoved) => numUnitsAlive = (ushort)max(0, numUnitsAlive - numRemoved);
        public void Add(int numAdded) => numUnitsAlive = (ushort)min(BaseNumUnits, numUnitsAlive + numAdded);

        public FormationData SetWidth(int newWidth)
        {
            width = (byte)min(maxRow, newWidth);
            depth = (byte)(numUnitsAlive / max(1,width));
            return this;
        }
        
        public FormationData SetDirection(float2 newDirection)
        {
            direction2DForward = half2(newDirection);
            return this;
        }
        
        public FormationData SetDirection(float3 firstUnitFirstRow, float3 lastUnitFirstRow)
        {
            float3 direction = cross(down(), normalizesafe(lastUnitFirstRow - firstUnitFirstRow));
            direction2DForward = half2(direction.xz);
            return this;
        }
    }
}
