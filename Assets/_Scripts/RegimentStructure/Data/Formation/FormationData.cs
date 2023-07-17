using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using half = Unity.Mathematics.half;

namespace KaizerWald
{
    [System.Serializable]
    public struct FormationData
    {
        //Taille actuelle 18 Bytes/Octets
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        //public readonly ushort BaseNumUnits;
        private readonly byte  minRow; 
        private readonly byte  maxRow;
        private readonly half2 unitSize;
        private readonly half  spaceBetweenUnits;

        private readonly ushort numUnitsAlive;
        private readonly byte   width;
        private readonly byte   depth;
        private readonly half2  direction2DForward;
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
        
        public readonly float2 Direction2DForward => direction2DForward;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public FormationData(int numUnits, int2 minMaxRow = default, float spaceBetweenUnit = 1f, float2 unitSize = default, float3 direction = default)
        {
            int numberUnits = max(1,numUnits);
            //BaseNumUnits = numUnitsAlive = (ushort)numberUnits;
            numUnitsAlive = (ushort)numberUnits;
            
            int2 minMax = minMaxRow.Equals(default) ? new int2(1, max(1, numberUnits / 2)) : minMaxRow;
            minRow = (byte)min(byte.MaxValue, minMax.x);
            maxRow = (byte)min(byte.MaxValue, minMax.y);
            
            this.unitSize = unitSize.Equals(default) ? half2(1) : (half2)unitSize;
            spaceBetweenUnits = (half)spaceBetweenUnit;
            
            width = maxRow;
            depth = (byte)ceil(numberUnits / max(1f,width));
            
            direction2DForward = direction.approximately(default) ? new half2(half.zero,half(1)) : half2(normalizesafe(direction).xz);
        }

        public FormationData(RegimentType regimentType, Vector3 direction)
        {
            RegimentClass regimentClass = regimentType.RegimentClass;
            //BaseNumUnits = numUnitsAlive = (ushort)regimentClass.BaseNumberUnit;
            numUnitsAlive = (ushort)regimentClass.BaseNumberUnit;
            
            minRow = (byte)min(byte.MaxValue, regimentClass.MinRow);
            maxRow = (byte)min(byte.MaxValue,regimentClass.MaxRow);
            
            unitSize = (half2)float2(regimentClass.Category.UnitSize.x, regimentClass.Category.UnitSize.z);
            spaceBetweenUnits = (half)regimentClass.SpaceBetweenUnits;
            
            width = maxRow;
            depth = (byte)ceil(regimentClass.BaseNumberUnit / max(1f,width)); //depth = (byte)(numUnitsAlive / width + 1);
            
            direction2DForward = half2(normalizesafe(direction).xz);
        }
        
        public FormationData(in FormationData otherFormation)
        {
            //BaseNumUnits = numUnitsAlive = otherFormation.BaseNumUnits;
            numUnitsAlive = otherFormation.numUnitsAlive;
            
            minRow =  (byte)min(byte.MaxValue, (int)otherFormation.minRow);
            maxRow = (byte)min(byte.MaxValue, (int)otherFormation.maxRow);
            unitSize = otherFormation.unitSize;
            spaceBetweenUnits = otherFormation.spaceBetweenUnits;
            
            width = otherFormation.width;
            depth = otherFormation.depth;
            
            direction2DForward = otherFormation.direction2DForward;
        }

        public FormationData(Formation formation)
        {
            //BaseNumUnits = numUnitsAlive = (ushort)formation.BaseNumUnits;
            numUnitsAlive = (ushort)formation.NumUnitsAlive;

            minRow = (byte)min(byte.MaxValue, formation.MinRow);
            maxRow = (byte)min(byte.MaxValue, formation.MaxRow);
            unitSize = half2(formation.UnitSize);
            spaceBetweenUnits = half(formation.SpaceBetweenUnits);

            width = (byte)formation.Width;
            depth = (byte)formation.Depth;

            direction2DForward = half2(formation.Direction2DForward);
        }
        
        public FormationData(Formation formation, int numUnits)
        {
            //BaseNumUnits = numUnitsAlive = (ushort)formation.BaseNumUnits;
            numUnitsAlive = (ushort)numUnits;

            minRow = (byte)min(byte.MaxValue, formation.MinRow);
            maxRow = (byte)min(byte.MaxValue, formation.MaxRow);
            unitSize = half2(formation.UnitSize);
            spaceBetweenUnits = half(formation.SpaceBetweenUnits);

            width = (byte)formation.Width;
            depth = (byte)formation.Depth;

            direction2DForward = half2(formation.Direction2DForward);
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ METHODS ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public readonly float3 Direction3DForward => new (direction2DForward.x,0,direction2DForward.y);
        public readonly float3 Direction3DLine => cross(up(), Direction3DForward);
        public readonly float2 DistanceUnitToUnit => UnitSize + SpaceBetweenUnits;
        public float DistanceUnitToUnitX => DistanceUnitToUnit.x;
        public float DistanceUnitToUnitY => DistanceUnitToUnit.y;
        
        // Needed for Rearrangement
        private readonly int CountUnitsLastLine => numUnitsAlive - NumCompleteLine * width;
        public readonly bool IsLastLineComplete => NumCompleteLine == depth;
        public readonly int NumCompleteLine => Depth * Width == numUnitsAlive ? depth : depth - 1;
        public readonly int NumUnitsLastLine => IsLastLineComplete ? width : CountUnitsLastLine;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Setters!! IMPOSSIBLE CAR LES STRUCT SONT PAR COPIES!! ◈◈◈◈◈◈                                   ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public static implicit operator FormationData(Formation rhs) => new FormationData(rhs);

        public override string ToString()
        {
            return $"Current formation:\r\n" +
                   //$"BaseNumUnits:{BaseNumUnits}\r\n" +
                   $"minRow {minRow}\r\n" +
                   $"maxRow {maxRow}\r\n" +
                   $"unitSize {unitSize}\r\n" +
                   $"spaceBetweenUnits {spaceBetweenUnits}\r\n" +
                   $"numUnitsAlive {numUnitsAlive}\r\n" +
                   $"width {width}\r\n" +
                   $"depth {depth}\r\n" +
                   $"direction2DForward {direction2DForward}\r\n";
        }
    }
}
