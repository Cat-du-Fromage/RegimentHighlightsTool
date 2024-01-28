using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using static Unity.Mathematics.float2;
using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace Kaizerwald
{
    public interface IFormationInfo
    {
        public int BaseNumUnits { get; } 
        public int2 MinMaxRow { get; } 
        public float2 UnitSize { get; } 
        public float SpaceBetweenUnit { get; } 
    }
    
    public class Formation
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public readonly int BaseNumUnits;
        public readonly int MinRow; 
        public readonly int MaxRow;
        public readonly float2 UnitSize;
        public readonly float SpaceBetweenUnits;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ Properties ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public int NumUnitsAlive { get; private set; }
        public int Width { get; private set; }
        public int Depth { get; private set; }
        public float3 DirectionForward { get; private set; }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public Formation(int numUnits)
        {
            int numberUnits = max(1,numUnits);
            BaseNumUnits = NumUnitsAlive = numberUnits;
            int2 minMax = new int2(1, max(1, numberUnits / 2));
            MinRow = (byte)min(byte.MaxValue, minMax.x);
            MaxRow = (byte)min(byte.MaxValue, minMax.y);
            UnitSize = float2(1);
            SpaceBetweenUnits = 1f;
            Width = MaxRow;
            Depth = (int)ceil(numberUnits / max(1f,Width));
            DirectionForward = forward();
        }

        public Formation(int numUnits, int2 minMaxRow = default, float2 unitSize = default, float spaceBetweenUnit = 1f, float3 direction = default)
        {
            int numberUnits = max(1,numUnits);
            BaseNumUnits = NumUnitsAlive = numberUnits;
            int2 minMax = minMaxRow.Equals(default) ? new int2(1, max(1, numberUnits / 2)) : minMaxRow;
            MinRow = (byte)min(byte.MaxValue, minMax.x);
            MaxRow = (byte)min(byte.MaxValue, minMax.y);
            UnitSize = unitSize.approximately(default) ? float2(1) : unitSize;
            SpaceBetweenUnits = spaceBetweenUnit;
            Width = MaxRow;
            Depth = (int)ceil(numberUnits / max(1f,Width));
            DirectionForward = direction.approximately(default) ? forward() : normalizesafe(direction);
        }
        
        public Formation(int numUnits, int2 minMaxRow = default, float2 unitSize = default, float3 direction = default, float spaceBetweenUnit = 1f)
        {
            int numberUnits = max(1,numUnits);
            BaseNumUnits = NumUnitsAlive = numberUnits;
            int2 minMax = minMaxRow.Equals(default) ? new int2(1, max(1, numberUnits / 2)) : minMaxRow;
            MinRow = (byte)min(byte.MaxValue, minMax.x);
            MaxRow = (byte)min(byte.MaxValue, minMax.y);
            UnitSize = unitSize.approximately(default) ? float2(1) : unitSize;
            SpaceBetweenUnits = spaceBetweenUnit;
            Width = MaxRow;
            Depth = (int)ceil(numberUnits / max(1f,Width));
            DirectionForward = direction.approximately(default) ? forward() : normalizesafe(direction);
        }
        
        public Formation(FormationData formationData)
        {
            BaseNumUnits = formationData.NumUnitsAlive;
            MinRow = (byte)formationData.MinRow;
            MaxRow = (byte)formationData.MaxRow;
            UnitSize = formationData.UnitSize;
            SpaceBetweenUnits = formationData.SpaceBetweenUnits;
            Width = formationData.Width;
            Depth = formationData.Depth;
            DirectionForward = formationData.Direction3DForward;
        }

        /*
        public Formation(RegimentType regimentType, Vector3 direction)
        {
            RegimentClass regimentClass = regimentType.RegimentClass;
            BaseNumUnits = NumUnitsAlive = regimentClass.BaseNumberUnit;
            MinRow = (byte)min(byte.MaxValue, regimentClass.MinRow);
            MaxRow = (byte)min(byte.MaxValue,regimentClass.MaxRow);
            UnitSize = new float2(regimentClass.Category.UnitSize.x, regimentClass.Category.UnitSize.z);
            SpaceBetweenUnits = regimentClass.SpaceBetweenUnits;
            Width = MaxRow;
            Depth = (int)ceil(regimentClass.BaseNumberUnit / max(1f,Width));
            DirectionForward = normalizesafe(direction);
        }
        */
        public Formation(IFormationInfo formationInfo, Vector3 direction)
        {
            BaseNumUnits = NumUnitsAlive = formationInfo.BaseNumUnits;
            MinRow = (byte)min(byte.MaxValue,formationInfo.MinMaxRow.x);
            MaxRow = (byte)min(byte.MaxValue,formationInfo.MinMaxRow.y);
            UnitSize = new float2(formationInfo.UnitSize.x, formationInfo.UnitSize.y);
            SpaceBetweenUnits = formationInfo.SpaceBetweenUnit;
            Width = MaxRow;
            Depth = (int)ceil(formationInfo.BaseNumUnits / max(1f,Width));
            DirectionForward = normalizesafe(direction);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ METHODS ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public int2 WidthDepth => new int2(Width, Depth);
        public float2 Direction2DForward => DirectionForward.xz;
        public float3 DirectionLine => cross(up(), DirectionForward);//cross(DirectionForward, up());
        public float2 DistanceUnitToUnit => UnitSize + SpaceBetweenUnits;
        public float DistanceUnitToUnitX => DistanceUnitToUnit.x;
        public float DistanceUnitToUnitY => DistanceUnitToUnit.y;
        public int LastRowFirstIndex => NumUnitsAlive - NumUnitsLastLine;
        
        // Needed for Rearrangement
        private int CountUnitsLastLine => NumUnitsAlive - NumCompleteLine * Width;
        public bool IsLastLineComplete => NumCompleteLine == Depth;
        public int NumCompleteLine => Depth * Width == NumUnitsAlive ? Depth : Depth - 1;
        public int NumUnitsLastLine => IsLastLineComplete ? Width : CountUnitsLastLine;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Setters ◈◈◈◈◈◈                                                                                      ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void Increment() => Add(1);
        public void Decrement() => DecreaseBy(1);
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Increase/Decrease Formation ◇◇◇◇◇◇                                                                 │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        public void Add(int numAdded)
        {
            NumUnitsAlive = min(BaseNumUnits, NumUnitsAlive + numAdded);
            Depth = (int)ceil(NumUnitsAlive / (float)Width);
        }

        public void DecreaseBy(int numRemoved)
        {
            NumUnitsAlive = max(0, NumUnitsAlive - numRemoved);
            Width = min(Width, NumUnitsAlive);
            Depth = (int)ceil(NumUnitsAlive / max(1f,Width));
        }

        public void SetWidth(int newWidth)
        {
            Width = min(MaxRow, newWidth);
            Depth = (int)ceil(NumUnitsAlive / max(1f,Width));
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Set Direction ◇◇◇◇◇◇                                                                               │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        public void SetDirection(float3 newDirection)
        {
            if (newDirection.IsZero()) return;
            DirectionForward = newDirection;
        }
        
        public void SetDirection(float2 newDirection)
        {
            SetDirection(new float3(newDirection.x, 0, newDirection.y));
        }

        public void SetDirection(float3 firstUnitFirstRow, float3 lastUnitFirstRow)
        {
            SetDirection(cross(down(), normalizesafe(lastUnitFirstRow - firstUnitFirstRow)));
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Overrides ◇◇◇◇◇◇                                                                                   │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Formation(FormationData rhs)
        {
            return new Formation(rhs);
        }
        
        public override string ToString()
        {
            return $"Current formation:\r\n" +
                   $"BaseNumUnits:{BaseNumUnits}\r\n" +
                   $"MinRow {MinRow}\r\n" +
                   $"MaxRow {MaxRow}\r\n" +
                   $"UnitSize {UnitSize}\r\n" +
                   $"SpaceBetweenUnits {SpaceBetweenUnits}\r\n" +
                   $"NumUnitsAlive {NumUnitsAlive}\r\n" +
                   $"Width {Width}\r\n" +
                   $"Depth {Depth}\r\n" +
                   $"Direction2DForward {Direction2DForward}\r\n";
        }
    }
}
