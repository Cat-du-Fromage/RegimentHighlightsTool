using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public sealed class MoveOrder : Order
    {
        public FormationData FormationDestination { get; private set; }
        public float3 LeaderDestination { get; private set; }
        
        public MoveOrder(FormationData formation, float3 leaderDestination) : base(EStates.Move)
        {
            FormationDestination = formation;
            LeaderDestination = leaderDestination;
        }
    }
}
