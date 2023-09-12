using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private readonly byte   minRow; 
        private readonly byte   maxRow;
        private readonly half2  unitSize;
        private readonly half   spaceBetweenUnits;

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
        public readonly float2 Direction2DBack => -(float2)direction2DForward;
        
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

        public FormationData(RegimentClass regimentClass, Vector3 direction)
        {
            numUnitsAlive = (ushort)regimentClass.BaseNumberUnit;
            
            minRow = (byte)min(byte.MaxValue, regimentClass.MinRow);
            maxRow = (byte)min(byte.MaxValue,regimentClass.MaxRow);
            
            unitSize = (half2)float2(regimentClass.Category.UnitSize.x, regimentClass.Category.UnitSize.z);
            spaceBetweenUnits = (half)regimentClass.SpaceBetweenUnits;
            
            width = maxRow;
            depth = (byte)ceil(regimentClass.BaseNumberUnit / max(1f,width));
            
            direction2DForward = half2(normalizesafe(direction).xz);
        }
        
        public FormationData(in FormationData otherFormation)
        {
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
            numUnitsAlive = (ushort)formation.NumUnitsAlive;

            minRow = (byte)min(byte.MaxValue, formation.MinRow);
            maxRow = (byte)min(byte.MaxValue, formation.MaxRow);
            unitSize = half2(formation.UnitSize);
            spaceBetweenUnits = half(formation.SpaceBetweenUnits);

            width = (byte)formation.Width;
            depth = (byte)ceil(numUnitsAlive / max(1f,width));

            direction2DForward = half2(formation.Direction2DForward);
        }
        
        public FormationData(Formation formation, int numUnits)
        {
            numUnitsAlive = (ushort)numUnits;

            minRow = (byte)min(byte.MaxValue, formation.MinRow);
            maxRow = (byte)min(byte.MaxValue, formation.MaxRow);
            unitSize = half2(formation.UnitSize);
            spaceBetweenUnits = half(formation.SpaceBetweenUnits);

            width = (byte)formation.Width;
            depth = (byte)ceil(numUnitsAlive / max(1f,width));

            direction2DForward = half2(formation.Direction2DForward);
        }
        
        public FormationData(FormationData formation, int numUnits)
        {
            numUnitsAlive = (ushort)numUnits;

            minRow = (byte)min(byte.MaxValue, formation.MinRow);
            maxRow = (byte)min(byte.MaxValue, formation.MaxRow);
            unitSize = half2(formation.UnitSize);
            spaceBetweenUnits = half(formation.SpaceBetweenUnits);

            width = (byte)formation.Width;
            depth = (byte)ceil(numUnitsAlive / max(1f,width));

            direction2DForward = half2(formation.Direction2DForward);
        }

        public FormationData(Formation formation, int newWidth, float3 direction)
        {
            numUnitsAlive = (ushort)formation.NumUnitsAlive;

            minRow = (byte)min(byte.MaxValue, formation.MinRow);
            maxRow = (byte)min(byte.MaxValue, formation.MaxRow);
            unitSize = half2(formation.UnitSize);
            spaceBetweenUnits = half(formation.SpaceBetweenUnits);

            width = (byte)min(newWidth, maxRow);
            depth = (byte)ceil(numUnitsAlive / max(1f,width));

            direction2DForward = half2(direction.xz);
        }
        
        public FormationData(Formation formation, int numUnits, int newWidth, float3 direction)
        {
            numUnitsAlive = (ushort)numUnits;

            minRow = (byte)min(byte.MaxValue, formation.MinRow);
            maxRow = (byte)min(byte.MaxValue, formation.MaxRow);
            unitSize = half2(formation.UnitSize);
            spaceBetweenUnits = half(formation.SpaceBetweenUnits);

            width = (byte)min(newWidth, maxRow);
            depth = (byte)ceil(numUnitsAlive / max(1f,width));

            direction2DForward = half2(direction.xz);
        }
        
        public FormationData(FormationData formation, int numUnits, int newWidth, float3 direction)
        {
            numUnitsAlive = (ushort)numUnits;

            minRow = (byte)min(byte.MaxValue, formation.MinRow);
            maxRow = (byte)min(byte.MaxValue, formation.MaxRow);
            unitSize = half2(formation.UnitSize);
            spaceBetweenUnits = half(formation.SpaceBetweenUnits);

            width = (byte)min(newWidth, maxRow);
            depth = (byte)ceil(numUnitsAlive / max(1f,width));

            direction2DForward = half2(direction.xz);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ METHODS ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public readonly float2 DistanceUnitToUnit => UnitSize + SpaceBetweenUnits;
        public float DistanceUnitToUnitX => DistanceUnitToUnit.x;
        public float DistanceUnitToUnitY => DistanceUnitToUnit.y;
        public int LastRowFirstIndex => NumUnitsAlive - NumUnitsLastLine;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Direction ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public readonly float3 Direction3DForward => new (direction2DForward.x,0,direction2DForward.y);
        public readonly float3 Direction3DBack => -Direction3DForward;
        public readonly float3 Direction3DLine => cross(up(), Direction3DForward);
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Rearrangement ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private readonly int CountUnitsLastLine => numUnitsAlive - NumCompleteLine * width;

        public readonly bool IsLastLineComplete => NumCompleteLine == depth;
        public readonly int NumCompleteLine => Depth * Width == numUnitsAlive ? depth : depth - 1;

        public readonly int NumUnitsLastLine => IsLastLineComplete ? width : CountUnitsLastLine;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Overrides ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FormationData(Formation rhs)
        {
            return new FormationData(rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FormationData lhs, FormationData rhs)
        {
            bool sameNumUnitsAlive = lhs.numUnitsAlive ==  rhs.numUnitsAlive;
            bool sameWidthDepth = lhs.width == rhs.width && lhs.depth == rhs.depth;
            bool isSameDirection = all(lhs.direction2DForward ==  rhs.direction2DForward);
            bool sameUnitSize = all(lhs.unitSize == rhs.unitSize);
            bool sameSpaceBetweenUnits = lhs.spaceBetweenUnits ==  rhs.spaceBetweenUnits;
            bool sameMinMaxRow = lhs.minRow == rhs.minRow && lhs.maxRow ==  rhs.maxRow;
            return sameNumUnitsAlive && sameWidthDepth && isSameDirection && sameUnitSize && sameSpaceBetweenUnits && sameMinMaxRow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FormationData lhs, FormationData rhs)
        {
            return !(lhs == rhs);
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Overrides ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public bool Equals(FormationData other)
        {
            bool rowsEqual = minRow == other.minRow && maxRow == other.maxRow;
            bool widthDepthEquals = width == other.width && depth == other.depth;
            return rowsEqual && widthDepthEquals &&
                   numUnitsAlive == other.numUnitsAlive && 
                   unitSize.Equals(other.unitSize) && 
                   spaceBetweenUnits.Equals(other.spaceBetweenUnits) && 
                   direction2DForward.Equals(other.direction2DForward);
        }

        public override bool Equals(object obj)
        {
            return obj is FormationData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(minRow, maxRow, unitSize, spaceBetweenUnits, numUnitsAlive, width, depth, direction2DForward);
        }
        
        public override string ToString()
        {
            return $"Current formation:\r\n" +
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
