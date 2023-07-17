using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public class UnitMoveOrder : UnitOrder
    {
        public readonly float3 Direction;
        public readonly float3 Destination;
        
        public UnitMoveOrder(Unit receiver, EStates state, float3 leaderDestination, FormationData formationData) : base(receiver, state)
        {
            Direction = formationData.Direction3DForward;
            Destination = formationData.GetUnitRelativePositionToRegiment3D(receiver.IndexInRegiment, leaderDestination);
        }

        public UnitMoveOrder(Unit receiver, MoveRegimentOrder baseOrder) 
            : this(receiver, baseOrder.StateOrdered, baseOrder.LeaderDestination, baseOrder.FormationDestination)
        {
            
        }
        
        public UnitMoveOrder(Unit receiver, UnitMoveOrder other) : base(receiver, other.StateOrdered)
        {
            Direction = other.Direction;
            Destination = other.Destination;
        }
    }
}
