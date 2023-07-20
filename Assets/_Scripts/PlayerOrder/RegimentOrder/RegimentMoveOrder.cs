/*

using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    public sealed class RegimentMoveOrder : RegimentOrder
    {
        public readonly float3 LeaderDestination;
        public Formation FormationDestination { get; private set; }
        
        public RegimentMoveOrder(Formation regimentCurrentFormation, int widthGoal, Vector3 firstUnitFirstRow, Vector3 lastUnitFirstRow) : base(EStates.Move)
        {
            FormationDestination = regimentCurrentFormation;
            FormationDestination.SetWidth(widthGoal);
            float3 direction = normalizesafe(cross(down(), lastUnitFirstRow - firstUnitFirstRow));
            FormationDestination.SetDirection(direction);
            LeaderDestination = (firstUnitFirstRow + lastUnitFirstRow) / 2;
        }
    }
}

*/