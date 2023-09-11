using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    //Difference with Move/Fire
    //Will automatically chase target if not at range
    //there will be a toggle button to change to Fire only so unit wont chase (similar to "Hold Line" in Total war)
    public class ChaseRegimentState : RegimentState
    {
        public Regiment TargetToChase { get; private set; }
        public float3 Destination { get; private set; }
        public bool LeaderReachDestination { get; private set; }
        private int AttackRange => RegimentAttach.RegimentType.Range;

        public ChaseRegimentState(RegimentStateMachine regimentStateMachine) : base(regimentStateMachine,EStates.Chase)
        {
            
        }

        public override void SetupState(Order order)
        {
            RangeAttackOrder rangeAttackOrder = (RangeAttackOrder)order;
            //1) Setup target to engage
            TargetToChase = rangeAttackOrder.TargetEnemyRegiment;
            //2) Establish distance?
        }

        public override void UpdateState()
        {
            Destination = EstablishDestination();
            if (LeaderReachDestination)
            {
                //FireMethod()
            }
            else
            {
                //MoveMethod()
            }
            //Si dstReach && a portee => Utiliser method
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
/*
        private void MoveRegiment()
        {
            if (LeaderReachDestination) return; // Units may still be on their way
            float3 direction = normalizesafe(Destination - Position);
            float3 translation = Time.deltaTime * MoveSpeed * direction;
            RegimentTransform.Translate(translation, Space.World);
            RegimentTransform.LookAt(Position + FormationDestination.Direction3DForward);
        }
*/

        private bool SwitchToFireState()
        {
            LeaderReachDestination = LeaderReachDestination || distance(Position, Destination) <= 0.01f;
            if (!LeaderReachDestination) return false;
            foreach (UnitStateMachine unitStateMachine in LinkedRegimentStateMachine.UnitsStateMachine)
            {
                if (!unitStateMachine.IsIdle) return false;
            }
            //LinkedRegimentStateMachine.ToDefaultState();
            return true;
        }

        private float3 EstablishDestination()
        {
            int unitIndexMin = GetUnitIndexMin();
            Unit unitTargeted = TargetToChase.Units[unitIndexMin];
            float3 targetPosition = unitTargeted.transform.position;
            
            //nous avons la cible => déterminer comment l'atteindre?
            float3 directionTargetToRegiment = normalizesafe(Position - targetPosition);
            float3 positionToReach = targetPosition + directionTargetToRegiment * AttackRange; //minimumm needed to shoot
            return positionToReach;
        }

        /*
        public void GetClosestUnit()
        {
            (int closestUnitIndex, float minDistance) = (-1, INFINITY);
            for (int i = 0; i < TargetToChase.UnitsTransform.Count; i++)
            {
                float3 unitPosition = TargetToChase.UnitsTransform[i].position;
                float distance = distancesq(Position, unitPosition);
                if (distance > minDistance) continue;
                (closestUnitIndex, minDistance) = (i, distance);
            }
            return closestUnitIndex;
        }
        */
        
        private int GetUnitIndexMin()
        {
            float3 position = Position;
            int numUnits = TargetToChase.UnitsTransform.Count;
            
            (int closestUnitIndex, float minDistance) = (-1, INFINITY);
            int midLength = numUnits / 2 + (numUnits & 1); //if unpair num Units add middle (mid values(dst and index) will be the same)
            for (int startIndex = 0; startIndex < midLength; startIndex++)
            {
                int2 startEndIndex = new int2(startIndex, numUnits - (1 + startIndex));
                float2 distanceStartEnd = new
                (
                    distancesq(position, TargetToChase.UnitsTransform[startEndIndex[0]].position),
                    distancesq(position, TargetToChase.UnitsTransform[startEndIndex[1]].position)
                );
                if (cmin(distanceStartEnd) > minDistance) continue;
                int closestId = distanceStartEnd[0] < distanceStartEnd[1] ? 0 : 1;
                (closestUnitIndex, minDistance) = (startEndIndex[closestId], distanceStartEnd[closestId]);
            }
            return closestUnitIndex;
        }
    }
}
