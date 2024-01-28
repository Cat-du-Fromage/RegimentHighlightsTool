using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;

namespace Kaizerwald
{
    public static class SelectionInfo
    {
        public static float2 GetMinMaxSelectionWidth(List<HighlightRegiment> selectedRegiments)
        {
            float2 minMaxDistance = float2.zero;
            foreach (HighlightRegiment selection in selectedRegiments)
            {
                minMaxDistance += GetMinMaxFormationLength(selection.CurrentFormation);
            }
            return minMaxDistance;
        }
        
        public static int GetTotalUnitsSelected(List<HighlightRegiment> selectedRegiments)
        {
            return selectedRegiments.Sum(regiment => regiment.CurrentFormation.NumUnitsAlive);
        }

        private static float2 GetMinMaxFormationLength(in FormationData formation)
        {
            return formation.DistanceUnitToUnit * max(1, formation.MinMaxRow - 1);
        }
        
        public static NativeArray<int> GetSelectionsMinWidth(List<HighlightRegiment> selectedRegiments)
        {
            NativeArray<int> tmp = new (selectedRegiments.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < selectedRegiments.Count; i++)
            {
                tmp[i] = selectedRegiments[i].CurrentFormation.MinRow;
            }
            return tmp;
        }
    }
}