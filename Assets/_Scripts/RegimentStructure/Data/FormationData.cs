using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    public struct FormationData
    {
        public readonly ushort BaseNumUnits;
        
        //Space between Unit
        private readonly half spaceBetweenUnits;
        public readonly float SpaceBetweenUnits => spaceBetweenUnits;
        
        //Unit Size
        private readonly half2 unitSize;
        public readonly float2 UnitSize => unitSize;
        
        //Units Alive Counter
        private ushort numUnitsAlive;
        public readonly int NumUnitsAlive => numUnitsAlive;
        
        //Width/Depth of the formation (Can be updated)
        private byte width, depth;
        public readonly int Width => width;
        public readonly int Depth => depth;

        //Min/Max Row allowed (Based can't be changed)
        private readonly byte minRow, maxRow;
        public readonly int MinRow => min((int)minRow, numUnitsAlive);
        public readonly int MaxRow => min((int)maxRow, numUnitsAlive);
        public readonly int2 MinMaxRow => new int2(MinRow, MaxRow);
        
        //Needed for Rearangemment
        public readonly int NumCompleteLine => depth * width == numUnitsAlive ? depth : depth - 1;
        //public readonly int NumCompleteLine => (int)floor(numUnitsAlive / (float)width); //REWORK LATER
        public readonly int LastLineNumUnit => numUnitsAlive - (NumCompleteLine * width);
        public readonly int NumUnitsLastLine => select(LastLineNumUnit,width,LastLineNumUnit == 0);
        
        public FormationData(RegimentType regimentType)
        {
            RegimentClass regimentClass = regimentType.RegimentClass;
            BaseNumUnits = numUnitsAlive = (ushort)regimentClass.BaseNumberUnit;
            minRow = (byte)min(byte.MaxValue, regimentClass.MinRow);
            maxRow = (byte)min(byte.MaxValue,regimentClass.MaxRow);
            
            unitSize = (half2)float2(regimentClass.Category.UnitSize.x, regimentClass.Category.UnitSize.z);
            spaceBetweenUnits = (half)regimentClass.SpaceBetweenUnits;
            
            width = maxRow;
            depth = (byte)(numUnitsAlive / width);
            Debug.Log($"width: {width}; depth: {depth}");
        }
        
        public readonly float2 DistanceUnitToUnit => UnitSize + SpaceBetweenUnits;
        public readonly int NumUnitLastLine => numUnitsAlive - width * (depth - 1);
        
        public void OnUnitDeath(int numRemoved)
        {
            numUnitsAlive -= (ushort)numRemoved;
            if (numUnitsAlive > width) return;
            SetWidth(numUnitsAlive);
        }
        
        public void SetWidth(int newWidth)
        {
            width = (byte)min(newWidth, NumUnitsAlive);
            depth = (byte)(numUnitsAlive / max(1,width));
        }
    }
}
