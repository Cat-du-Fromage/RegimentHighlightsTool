using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public class UnitMoveOrder : UnitOrder
    {
        public float3 Destination { get; private set; } // Peut être calculé via FormationData
        public FormationData FormationDestination { get; private set; }
        
        public UnitMoveOrder(int indexInRegiment, float3 leaderDestination, FormationData formationData) : base(EStates.Move)
        {
            FormationDestination = formationData;
            Destination = formationData.GetUnitRelativePositionToRegiment3D(indexInRegiment, leaderDestination);
        }

        public UnitMoveOrder(int indexInRegiment, RegimentMoveOrder other) : base(other.StateOrdered)
        {
            FormationDestination = other.FormationDestination;
            Destination = FormationDestination.GetUnitRelativePositionToRegiment3D(indexInRegiment, other.LeaderDestination);
        }
        
        public UnitMoveOrder(UnitMoveOrder other) : base(other.StateOrdered)
        {
            FormationDestination = other.FormationDestination;
            Destination = other.Destination;
        }
        
        public UnitMoveOrder(FormationData formationData, float3 position) : base(EStates.Move)
        {
            FormationDestination = formationData;
            Destination = position;
        }
    }
}
