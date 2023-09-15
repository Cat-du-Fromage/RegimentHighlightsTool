using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

using static KaizerWald.KzwMath;
using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static Unity.Mathematics.quaternion;

namespace KaizerWald
{
    public static class FormationUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 GetLastLineOffset(bool isLastLineComplete, int width, int numUnitsLastLine, float distanceUnitToUnitX, float3 lineDirection)
        {
            if (isLastLineComplete) return zero;
            float offset = (width - numUnitsLastLine) / 2f;
            float3 offsetPosition = lineDirection * (offset * distanceUnitToUnitX);
            return offsetPosition;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetLastLineOffset(this Formation formation)
        {
            return GetLastLineOffset(formation.IsLastLineComplete, formation.Width, formation.NumUnitsLastLine, 
                formation.SpaceBetweenUnits, formation.DirectionLine);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetLastLineOffset(this FormationData formation, float spaceSize)
        {
            return GetLastLineOffset(formation.IsLastLineComplete, formation.Width, 
                formation.NumUnitsLastLine, spaceSize, formation.Direction3DLine);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexAround(List<Unit> units, int index, in FormationData formation)
        {
            return GetIndexAround(units.ToArray(), index, formation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexAround(ReadOnlySpan<Unit> unitsSpan, int index, FormationData formation)
        {
            // --------------------------------------------------------------
            //Better to cache value in case we have to refactor FormationData
            (int width, int depth) = (formation.Width, formation.Depth);
            (int numUnits, int lastUnitsIndex) = (formation.NumUnitsAlive, formation.NumUnitsAlive-1);

            //DebugFormation($"{index} formation.NumUnitsAlive: {formation.NumUnitsAlive} || lastUnitsIndex: {lastUnitsIndex}");
            // --------------------------------------------------------------
            
            int lastDepthIndex = depth - 1;
            (int coordX, int coordY) = GetXY(index, width);
            
            if (coordY == lastDepthIndex) return RearrangeInline(unitsSpan, coordX, coordY, formation);
            
            int numLineBehind = depth - coordY;
            return CheckLineBehinds(unitsSpan/*, formation*/);
            // ---------------------------------------------
            // INTERNAL METHODS
            // ---------------------------------------------
            int CheckLineBehinds(ReadOnlySpan<Unit> units/*, in FormationData formation*/)
            {
                //int indexUnit = -1;
                //DebugFormation("Passe in CheckLineBehinds ", index == 23);
                for (int line = 1; line < numLineBehind; line++)
                {
                    //DebugFormation("Passe in CheckLineBehinds FOR1", index == 23);
                    int lineIndexChecked = coordY + line;
                    bool isCurrentLineCheckedLast = lineIndexChecked == lastDepthIndex;
                    
                    int lineWidth = isCurrentLineCheckedLast ? formation.NumUnitsLastLine : width;
                    int lastLineIndexChecked = lineWidth - 1;

                    bool2 leftRightClose = new (coordX == 0, coordX == lastLineIndexChecked);
                    if (!IsNextRowValid(units, coordY, formation)) continue; //We first check if there is something to check behind
                    
                    int indexUnit = -1;
                    for (int i = 0; i <= lineWidth; i++) // 0 because we check unit right behind
                    {
                        //DebugFormation($"{i} Passe in CheckLineBehinds FOR2", index == 23);
                        if (i == 0) //Check Unit Behind
                        {
                            indexUnit = mad(lineIndexChecked, width, coordX);
                            //Check if we pick on the last line: then adjust considering if the line is complete or not
                            if (lineIndexChecked == lastDepthIndex)
                            {
                                int unitBehindIndex = GetUnitBehindInUnEvenLastLine(indexUnit, formation);
                                indexUnit = formation.IsLastLineComplete ? min(indexUnit, lastUnitsIndex) : unitBehindIndex;
                                //DebugFormation($"{index} Check Unit Behind (lineIndexChecked == lastDepthIndex) | unitBehindIndex: {unitBehindIndex} IsLastLineComplete: {formation.IsLastLineComplete}", indexUnit >= units.Length);
                            }
                            //DebugFormation($"{index} Check Unit Behind || IsUnitValid: {indexUnit}", indexUnit >= units.Length);
                            if (IsUnitValid(units[indexUnit])) return indexUnit;
                            leftRightClose = IsLeftRightClose(indexUnit, numUnits, lineWidth);
                            continue;
                        }
                        if (!leftRightClose.x) //Check Left/Negative Index
                        {
                            int x = min(coordX, lineWidth) - i;
                            indexUnit = GetIndex(int2(x, lineIndexChecked), width);
                            
                            //DebugFormation($"{index} Check Left/Negative Index || IsUnitValid: {indexUnit}", indexUnit >= units.Length);
                            if (IsUnitValid(units[indexUnit])) return indexUnit;
                            leftRightClose.x = min(coordX, lineWidth) - i == 0;
                        }
                        if (!leftRightClose.y) //Check Right/Positiv Index
                        {
                            indexUnit = GetIndex(int2(coordX + i, lineIndexChecked), width);

                            //DebugFormation($"{index} Check Right/Positiv Index || IsUnitValid: {indexUnit}", indexUnit >= units.Length);
                            
                            if(IsUnitValid(units[indexUnit])) return indexUnit;
                            leftRightClose.y = coordX + i == lastLineIndexChecked;
                        }
                        if (all(leftRightClose)) break; //No more unit to check in this line
                    }
                }
                return -1;
            }
        }

        private static void DebugFormation(string message, bool checkCondition = true)
        {
            if(checkCondition) Debug.Log(message);
        }

        private static bool IsInBounds(int unitIndex, int unitsCount) => unitIndex < unitsCount;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsUnitValidSafe(Unit unit) => unit != null && !unit.IsDead;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsUnitValid(Unit unit) => !unit.IsDead;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RearrangeInline(ReadOnlySpan<Unit> units, int coordX, int coordY, in FormationData formation)
        {
            int increment = 1;
            int fullIndex = mad(coordY, formation.Width, coordX);
            while (fullIndex < formation.NumUnitsAlive - 1)
            {
                int xToCheck = coordX + increment;
                fullIndex = mad(coordY, formation.Width, xToCheck);
                //DebugFormation($"Start was: {mad(coordY, formation.Width, coordX)} | RearrangeInline IsUnitValid: {fullIndex}", fullIndex >= units.Length);
                if (IsUnitValid(units[fullIndex])) return fullIndex;
                increment++;
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetUnitBehindInUnEvenLastLine(int index, in FormationData formation)
        {
            int offset = (int)ceil((formation.Width - formation.NumUnitsLastLine) / 2f);
            int indexUnitBehind = index - offset;
            int minIndex = formation.Width * (formation.Depth - 1);
            indexUnitBehind = max(minIndex, indexUnitBehind);
            indexUnitBehind = min(formation.NumUnitsAlive - 1, indexUnitBehind);
            return indexUnitBehind;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNextRowValid(ReadOnlySpan<Unit> units, int yCoordLineChecked, in FormationData formation)
        {
            int nextYLine = yCoordLineChecked + 1;
            bool isOutOfBound = nextYLine > formation.Depth - 1;
            //DebugFormation($"IsNextRowValid: {isOutOfBound} | nextYLine: {nextYLine} | formation.Depth - 1: {formation.Depth - 1}");
            if (isOutOfBound) return false;
            foreach (Unit unit in units)
            {
                if (IsUnitValid(unit)) return true;
            }
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool2 IsLeftRightClose(int index, int numUnits, int lineWidth)
        {
            return new bool2
            {
                x = index == numUnits - lineWidth,
                y = index == numUnits - 1
            };
        }
    }
}
