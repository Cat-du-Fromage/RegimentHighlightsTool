using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public enum EMoveType
    {
        March,
        Run,
    }
    
    public sealed class MoveOrder : Order
    {
        public EMoveType MoveType { get; private set; }
        public FormationData FormationDestination { get; private set; }
        public float3 LeaderDestination { get; private set; }
        
        public MoveOrder(FormationData formation, float3 leaderDestination, EMoveType moveType = EMoveType.Run) : base(EStates.Move)
        {
            FormationDestination = formation;
            LeaderDestination = leaderDestination;
            MoveType = moveType;
        }

        public override string ToString()
        {
            return $"LeaderDestination: {LeaderDestination}\n {FormationDestination.ToString()}";
        }
    }
}
