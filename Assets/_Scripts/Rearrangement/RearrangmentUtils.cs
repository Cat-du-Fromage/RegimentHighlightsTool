using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    public static class RearrangmentUtils
    {
        private static bool IsUnitValid(HashSet<Unit> unitKilled, Unit unit)
        {
            return unit != null && !unitKilled.Contains(unit);
        }
        
        public static int RearrangeInline(HashSet<Unit> unitKilled, Unit[] units, int index)
        {
            if (index == units.Length - 1) return -1;
            int maxIteration = units.Length - index;
            for (int i = 1; i < maxIteration; i++) //Begin at 1, so we start at the next index
            {
                int indexToCheck = index + i;
                if (IsUnitValid(unitKilled, units[indexToCheck])) return indexToCheck;
            }
            return -1;
        }
        
        private static bool IsNextRowValid(HashSet<Unit> unitKilled, Unit[] units, int yLine, in FormationData formation)
        {
            int nextYLineIndex = yLine + 1;
            int lastLineIndex = formation.Depth - 1;
            
            if (nextYLineIndex > lastLineIndex) return false;
            int numUnitOnLine = select(formation.Width, formation.NumUnitsAlive, nextYLineIndex == lastLineIndex);

            Unit[] lineToCheck = new Unit[numUnitOnLine];
            Array.Copy(units, nextYLineIndex * formation.Width, lineToCheck, 0, numUnitOnLine);
            
            int numValid = 0;
            foreach (Unit unit in lineToCheck)
            {
                numValid += select(0,1,IsUnitValid(unitKilled, unit));
            }
            return numValid > 0;
        }

        private static bool IsNextRowValid(HashSet<Unit> unitKilled, Unit[] units, int yLine, int unitsPerLine, int numLine, int numUnitsLastLine)
        {
            int nextYLineIndex = yLine + 1;
            int lastLineIndex = numLine - 1;

            if (nextYLineIndex > lastLineIndex) return false;
            int numUnitOnLine = select(unitsPerLine,numUnitsLastLine,nextYLineIndex == lastLineIndex);

            Unit[] lineToCheck = new Unit[numUnitOnLine];
            Array.Copy(units, nextYLineIndex * unitsPerLine, lineToCheck, 0, numUnitOnLine);
            
            foreach (Unit unit in lineToCheck)
            {
                if (IsUnitValid(unitKilled, unit)) return true;
            }
            return false;
        }

        private static int GetIndexBehind(Unit[] units, int xInRegiment, int yInRegiment,int unitsPerLine)
        {
            int indexUnitBehind = mad(yInRegiment + 1, unitsPerLine, xInRegiment);
            int minIndex = unitsPerLine * (yInRegiment + 1);
            indexUnitBehind = max(minIndex, indexUnitBehind);
            indexUnitBehind = min(minIndex + unitsPerLine - 1, indexUnitBehind);
            return select(-1,indexUnitBehind,indexUnitBehind < units.Length) ;
        }

        public static int GetIndexAround(HashSet<Unit> unitsKilled, Unit[] units, int index, in FormationData formation)
        {
            return GetIndexAround(unitsKilled, units, index, formation.Depth, formation.Width, formation.NumUnitsLastLine);
        }

        public static int GetIndexAround(HashSet<Unit> unitsKilled, Unit[] units, int index, int numLine, int unitsPerLine, int numUnitsLastLine)
        {
            int indexUnit = -1;
            int lastLineIndex = numLine - 1;
            
            int yInRegiment = index / unitsPerLine;
            int xInRegiment = index - (yInRegiment * unitsPerLine);
            
            //Inline if there is only ONE line
            bool nextRowValid = IsNextRowValid(unitsKilled, units, yInRegiment, unitsPerLine, numLine, numUnitsLastLine);
            if (numLine == 1 || yInRegiment == lastLineIndex || !nextRowValid) return RearrangeInline(unitsKilled, units, index);
            
            for (int lineIndex = yInRegiment + 1; lineIndex < numLine; lineIndex++) //Tester avec ++lineIndex (enlever le +1 à yRegiment)
            {
                int lineWidth = select(unitsPerLine, numUnitsLastLine,lineIndex == lastLineIndex);
                int lastXCoordCurrentLine = lineWidth - 1;
                
                //ATTENTION: LA DERNIERE LIGNE SI Composé uniquement d'entité null ne sera pas considéré comme VIDE DONC JAMAIS INLINE!
                indexUnit = GetIndexBehind(units, xInRegiment, yInRegiment, unitsPerLine);
                if (indexUnit != -1 && IsUnitValid(unitsKilled, units[indexUnit])) return indexUnit;
                
                bool2 leftRightClose = new (xInRegiment == 0, xInRegiment == lastXCoordCurrentLine);//-1 because we want the index
                for (int i = 0; i <= lineWidth; i++) // 0 because we check unit right behind
                {
                    if (IsRightValid(i)) return indexUnit;
                    if (IsLeftValid(i)) return indexUnit;
                    if (all(leftRightClose)) break;
                }
                
                bool IsRightValid(int i)
                {
                    if (leftRightClose.x) return false;
                    int x = min(xInRegiment, lastXCoordCurrentLine); //if line uneven, we readjust x so it's not out of bounds
                    indexUnit = mad(lineIndex, unitsPerLine, x-i);
                    leftRightClose.x = x - i == 0;
                    return IsUnitValid(unitsKilled, units[indexUnit]);
                }

                bool IsLeftValid(int i)
                {
                    if (leftRightClose.y) return false;
                    int x = min(xInRegiment, lastXCoordCurrentLine);
                    indexUnit = mad(lineIndex, unitsPerLine, x+i);
                    leftRightClose.y = x+i >= lastXCoordCurrentLine;
                    return IsUnitValid(unitsKilled, units[indexUnit]);
                }
            }
            return -1;
        }
    }
}
