using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    public sealed class MoveRegimentOrder : RegimentOrder
    {
        public readonly Vector3 LeaderDestination;
        public Formation FormationDestination { get; private set; }
        
        public MoveRegimentOrder(Regiment regiment,EStates stateOrdered,int widthGoal, Vector3 firstUnitFirstRow, Vector3 lastUnitFirstRow) : base(regiment, stateOrdered)
        {
            FormationDestination = regiment.CurrentFormation;
            FormationDestination.SetWidth(widthGoal);
            float3 direction = normalizesafe(cross(down(), lastUnitFirstRow - firstUnitFirstRow));
            FormationDestination.SetDirection(direction);
            LeaderDestination = (firstUnitFirstRow + lastUnitFirstRow) / 2;
        }
    }
}