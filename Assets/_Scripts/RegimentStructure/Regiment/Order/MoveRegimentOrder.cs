﻿using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public sealed class MoveRegimentOrder : RegimentOrder
    {
        public readonly Vector3 LeaderDestination;
        public FormationData FormationDestination { get; private set; }
        public MoveRegimentOrder(Regiment regiment,EStates stateOrdered,int widthGoal, Vector3 leaderDestination) : base(regiment, stateOrdered)
        {
            FormationDestination = regiment.CurrentFormation;
            FormationDestination.SetWidth(widthGoal);
            FormationDestination.SetDirection(math.normalizesafe(leaderDestination - regiment.transform.position));
            LeaderDestination = leaderDestination;
        }
        
        //Probleme ICI
        public MoveRegimentOrder(Regiment regiment,EStates stateOrdered,int widthGoal, Vector3 firstUnitFirstRow, Vector3 lastUnitFirstRow) : base(regiment, stateOrdered)
        {
            FormationDestination = regiment.CurrentFormation;
            FormationDestination.SetWidth(widthGoal);
            Vector3 direction = math.normalizesafe(math.cross(math.down(), lastUnitFirstRow - firstUnitFirstRow));
            Debug.Log($"MoveRegimentOrder Constructor: direction = {direction}");
            FormationDestination.SetDirection((float3)direction);
            Debug.Log($"MoveRegimentOrder Constructor: FormationDestination = {FormationDestination.ToString()}");
            LeaderDestination = (firstUnitFirstRow + lastUnitFirstRow)/2 + direction * regiment.CurrentFormation.DistanceUnitToUnit.y;
        }
    }
}