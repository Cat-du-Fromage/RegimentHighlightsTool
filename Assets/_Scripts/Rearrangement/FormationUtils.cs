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
        public static int NumUnitsToAdd(this List<Regiment> selectedRegiments, in float3 startPosition, in float3 endPosition)
        {
            float lineLength = length(endPosition - startPosition);
            float minRegimentsFormationSize = selectedRegiments.MinSizeFormation();
            
            int numUnitToAdd = (int)(lineLength - minRegimentsFormationSize);
            return max(0, numUnitToAdd);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MinSizeFormation(this List<Regiment> selectedRegiments)
        {
            float min = 0;
            float mediumSize = 0;
            for (int i = 0; i < selectedRegiments.Count; i++)
            {
                Formation formation = selectedRegiments[i].CurrentFormation;
                float unitSpace = formation.SpaceBetweenUnits;
                min += unitSpace * formation.MinRow;
                mediumSize += formation.UnitSize.x / 2f;
                if (i == 0) continue;
                float distanceBetweenUnit = selectedRegiments[i - 1].CurrentFormation.SpaceBetweenUnits;
                float spaceBetweenRegiment = distanceBetweenUnit + unitSpace / 2f;
                min += spaceBetweenRegiment;
            }
            mediumSize /= selectedRegiments.Count;
            return min - mediumSize;
        }

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

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetLastLineOffset(this Formation formation, int newUnitPerLine, in float3 newLineDirection)
        {
            int lastLineNumUnit = GetNumUnitsLastLine(formation.NumUnitsAlive, newUnitPerLine);
            return GetLastLineOffset(lastLineNumUnit == newUnitPerLine, newUnitPerLine, 
                lastLineNumUnit, formation.DistanceUnitToUnitX, newLineDirection);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetLastLineOffset(this FormationData formation, in float3 newLineDirection)
        {
            int lastLineNumUnit = GetNumUnitsLastLine(formation.NumUnitsAlive, formation.Width);
            
            if (lastLineNumUnit == formation.Width) return zero;
            float offset = (formation.Width - lastLineNumUnit) / 2f;
            float3 offsetPosition = (newLineDirection * formation.DistanceUnitToUnitX) * offset;
            return offsetPosition;
        }
        */
        
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3[] GetRegimentPlacement(this List<Regiment> selectedRegiments, in float3 startPosition, in float3 endPosition)
        {
            List<float3> positions = new List<float3>(2);

            int numUnitToAdd = selectedRegiments.NumUnitsToAdd(startPosition, endPosition);
            
            float3 lineDirection = normalizesafe(endPosition - startPosition);
            float3 columnDirection = normalizesafe(cross(lineDirection, down()));
            
            float offsetRegiment = 0;
            for (int i = 0; i < selectedRegiments.Count; i++)
            {
                Formation formation = selectedRegiments[i].CurrentFormation;

                float unitSpaceSize = formation.DistanceUnitToUnitX;
                
                offsetRegiment += i == 0 ? 0 : selectedRegiments[i - 1].CurrentFormation.DistanceUnitToUnitX;
                
                //what we actually update
                int newNumUnitPerLine = min(formation.MaxRow, formation.MinRow + numUnitToAdd);
                int newNumLine = GetTotalLine(formation.NumUnitsAlive, newNumUnitPerLine);
                int newNumUnitLastLine = GetNumUnitsLastLine(formation.NumUnitsAlive, newNumUnitPerLine);
                float3 offsetPosition = formation.GetLastLineOffset(newNumUnitPerLine, lineDirection);

                float3[] formationPositions = new float3[formation.NumUnitsAlive];
                for (int j = 0; j < formation.NumUnitsAlive; j++)
                {
                    (int x, int y) = KzwMath.GetXY(j,newNumUnitPerLine);
                    float3 linePosition = startPosition + lineDirection * (unitSpaceSize * x);
                    linePosition += lineDirection * offsetRegiment;
                    bool isLastRow = y == newNumLine - 1 && newNumUnitLastLine != newNumUnitPerLine;
                    linePosition += select(zero, offsetPosition, isLastRow);
                    float3 columnPosition = columnDirection * (unitSpaceSize * y);
                    //Position Here!
                    formationPositions[j] = linePosition + columnPosition;
                }
                positions.AddRange(formationPositions);
                offsetRegiment += (newNumUnitPerLine * unitSpaceSize);
            }
            return positions.ToArray();
        }
        */
        
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNumUnitsLastLine(int numUnits, int numUnitPerLine)
        {
            //======================================================================================
            //Lines information
            float regimentNumLine = numUnits / (float)numUnitPerLine;
            int numCompleteLine = (int)floor(regimentNumLine);
            //======================================================================================
            int lastLineNumUnit = numUnits - (numCompleteLine * numUnitPerLine);
            return select(lastLineNumUnit,numUnitPerLine,lastLineNumUnit == 0);
        }
        */
        
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTotalLine(int numUnits, int numUnitPerLine)
        {
            float formationNumLine = numUnits / (float)numUnitPerLine;
            int totalLine = (int)ceil(formationNumLine);
            return totalLine;
        }
        */
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RearrangeInline(IReadOnlyList<Unit> units, int coordX, int coordY, in FormationData formation)
        {
            int increment = 1;
            int fullIndex = mad(coordY, formation.Width, coordX);
            while (fullIndex < formation.NumUnitsAlive - 1)
            {
                int xToCheck = coordX + increment;
                fullIndex = mad(coordY, formation.Width, xToCheck);
                if(IsUnitValid(units[fullIndex])) return fullIndex;
                increment++;
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexAround(this List<Unit> units, int index, in FormationData formation)
        {
            return units.ToArray().GetIndexAround(index, formation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexAround(this Unit[] units, int index, in FormationData formation)
        {
            int lastLineIndex = formation.Depth - 1;
            (int width, int depth) = (formation.Width, formation.Depth);
            (int numUnits, int lastUnitsIndex) = (formation.NumUnitsAlive, formation.NumUnitsAlive-1);

            (int coordX, int coordY) = GetXY(index, formation.Width);
            
            if (coordY == lastLineIndex) return RearrangeInline(units, coordX, coordY, formation);
            
            int numLineBehind = depth - coordY;
            
            return CheckLineBehinds(formation);

            // ---------------------------------------------
            // INTERNAL METHODS
            // ---------------------------------------------
            int CheckLineBehinds(in FormationData formation)
            {
                //int indexUnit = -1;
                for (int line = 1; line < numLineBehind; line++)
                {
                    int lineIndexChecked = coordY + line;
                    bool isCurrentLineCheckedLast = lineIndexChecked == lastLineIndex;
                    
                    int lineWidth = isCurrentLineCheckedLast ? formation.NumUnitsLastLine : width;
                    int lastLineIndexChecked = lineWidth - 1;

                    bool2 leftRightClose = new (coordX == 0, coordX == lastLineIndexChecked);
                    if (!IsNextRowValid(units, coordY, formation)) continue; //We first check if there is something to check behind
                    
                    int indexUnit = -1;
                    for (int i = 0; i <= lineWidth; i++) // 0 because we check unit right behind
                    {
                        if (i == 0) //Check Unit Behind
                        {
                            indexUnit = mad(lineIndexChecked, width, coordX);
                            //Check if we pick on the last line: then adjust considering if the line is complete or not
                            if (lineIndexChecked == lastLineIndex)
                            {
                                int unitBehindIndex = GetUnitBehindInUnEvenLastLine(indexUnit, formation);
                                indexUnit = formation.IsLastLineComplete ? min(indexUnit, lastUnitsIndex) : unitBehindIndex;
                            }
                            if (IsUnitValid(units[indexUnit])) return indexUnit;
                            leftRightClose = IsLeftRightClose(indexUnit, numUnits, lineWidth);
                            continue;
                        }
                        if (!leftRightClose.x) //Check Left/Negative Index
                        {
                            int x = min(coordX, lineWidth) - i;
                            indexUnit = GetIndex(int2(x, lineIndexChecked), width);
                            if (IsUnitValid(units[indexUnit])) return indexUnit;
                            leftRightClose.x = min(coordX, lineWidth) - i == 0;
                        }
                        if (!leftRightClose.y) //Check Right/Positiv Index
                        {
                            indexUnit = GetIndex(int2(coordX + i, lineIndexChecked), width);
                            if(IsUnitValid(units[indexUnit])) return indexUnit;
                            leftRightClose.y = coordX + i == lastLineIndexChecked;
                        }
                        if (all(leftRightClose)) break; //No more unit to check in this line
                    }
                }
                return -1;
            }
        }
        
        
        private static int GetUnitBehind<T>(this T[] units, int2 coord, FormationData formation, ref bool2 leftRightClose, Func<T, bool> conditionCheck)
        {
            int indexUnit = GetIndex(int2(coord.x, coord.y), formation.Width);
                    
            bool lastLineComplete = formation.NumCompleteLine == formation.Width;
            //TODO: Totally different : need to Externalise
            if (coord.y == formation.Depth - 1 && !lastLineComplete)
            {
                indexUnit = GetUnitBehindInUnEvenLastLine(indexUnit, formation);
                if (conditionCheck(units[indexUnit])) return indexUnit;
                leftRightClose = IsLeftRightClose(indexUnit, formation.NumUnitsAlive, formation.NumUnitsLastLine);
            }
            else
            {
                indexUnit = min(indexUnit, formation.NumUnitsAlive - 1);
                if (conditionCheck(units[indexUnit])) return indexUnit;
                leftRightClose = new (coord.x == 0, coord.x == formation.Width - 1);
            }

            return -1;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsUnitValid(Unit unit) => unit != null && !unit.IsDead;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool2 IsLeftRightClose(int index, int numUnits, int lineWidth)
        {
            return new bool2
            {
                x = index == numUnits - lineWidth,
                y = index == numUnits - 1
            };
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
        private static bool IsNextRowValid(this Unit[] units, int yCoordLineChecked, in FormationData formation)
        {
            int nextYLine = yCoordLineChecked + 1;
            int totalLine = formation.Depth;
            int lastLineIndex = totalLine-1;

            bool isOutOfBound = nextYLine > totalLine - 1;
            if (isOutOfBound) return false;
            
            bool isNextLineLast = nextYLine == lastLineIndex;
            int numUnitOnLine = isNextLineLast ? formation.NumUnitsLastLine : formation.Width;
            
            ReadOnlySpan<Unit> lineToCheck = new (units,nextYLine * formation.Width, numUnitOnLine);
            //int numInvalid = 0;
            foreach (Unit unit in lineToCheck)
            {
                if (IsUnitValid(unit)) return true;
                //numInvalid += !IsUnitValid(unit) ? 1 : 0; //numInvalid += select(0,1,!IsUnitValid(unit));
            }
            return false;
            //return numInvalid != lineToCheck.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetClosestAround<T>(this T[] array, Func<T,bool> conditionCheck)
        {
            if (conditionCheck(array[0]))
            {
                
            }
        }
        //TODO: Breath Search: we search in a circle around the unit (use KwGrid:AdjacentCell)
        
        //TODO: Line Search(Rearrangment): search inline for the closest Unit
    }
}
