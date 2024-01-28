using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Kaizerwald.KzwMath;
using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Mathematics.math;
using static Kaizerwald.UnityMathematicsExtension;
using static Kaizerwald.CSharpContainerUtils;

namespace Kaizerwald
{
    public sealed class Regiment_MoveState : RegimentStateBase
    {
        private readonly int MarchSpeed;
        private readonly int RunSpeed;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool LeaderReachDestination { get; private set; }
        public int SpeedModifier { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Getters ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public float Speed => RegimentAttach.RegimentType.Speed;
        public float MoveSpeed => Speed * SpeedModifier;
        public bool IsRunning => SpeedModifier == RunSpeed;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setters ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void SetSpeedModifier(EMoveType moveType) => SpeedModifier = moveType == EMoveType.Run ? RunSpeed : MarchSpeed;
        private int SetMarching() => SpeedModifier = MarchSpeed;
        private int SetRunning() => SpeedModifier = RunSpeed;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment_MoveState(RegimentBehaviourTree behaviourTree) : base(behaviourTree,EStates.Move)
        {
            MarchSpeed = 1;
            RunSpeed = 3;
        }

        public override bool ConditionEnter()
        {
            return base.ConditionEnter();
        }

        public override void OnSetup(Order order)
        {
            MoveOrder moveOrder = (MoveOrder)order;
            RegimentBlackboard.OnOrder(order);
            SetSpeedModifier(moveOrder.MoveType);
        }

        public override void OnEnter()
        {
            LeaderReachDestination = false;
            UpdateDestinationReach();
            
            //SOIT on check qu'on est en "chasse" ici et on assign:
            AssignIndexToUnits(RegimentBlackboard.DestinationFormation);
            RegimentAttach.CurrentFormation.SetWidth(RegimentBlackboard.DestinationFormation.Width);
            RegimentAttach.CurrentFormation.SetDirection(RegimentBlackboard.DestinationFormation.Direction3DForward);
        }

        public override void OnUpdate()
        {
            if (LeaderReachDestination) return;
            MoveRegiment();
        }

        public override void OnExit()
        {
            //
        }

        public override EStates ShouldExit()
        {
            UpdateDestinationReach();
            if (!LeaderReachDestination) return StateIdentity;
            return RegimentBlackboard.IsChasing ? EStates.Fire : EStates.Idle;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void UpdateDestinationReach()
        {
            if (LeaderReachDestination) return;
            LeaderReachDestination = distance(Position, RegimentBlackboard.Destination) <= 0.01f;
        }

        private void MoveRegiment()
        {
            if (LeaderReachDestination) return; // Units may still be on their way
            float3 direction = normalizesafe(RegimentBlackboard.Destination - Position);
            float3 translation = Time.deltaTime * MoveSpeed * direction;
            BehaviourTree.CachedTransform.Translate(translation, Space.World);
            BehaviourTree.CachedTransform.LookAt(Position + RegimentBlackboard.DestinationFormation.Direction3DForward);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ On Enter ◈◈◈◈◈◈                                                                                         ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        private void AssignIndexToUnits(in FormationData formation)
        {
            NativeArray<float2> destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination.xz);
            List<Unit> tempUnitList = new List<Unit>(RegimentAttach.Units);
            for (int i = 0; i < destinations.Length; i++)
            {
                int closestUnitIndex = FindClosestUnitIndex(tempUnitList, destinations[i]);
                if (closestUnitIndex == -1) continue;
                tempUnitList[closestUnitIndex].SetIndexInRegiment(i);
                tempUnitList.RemoveAt(closestUnitIndex);
            }
        }

        private int FindClosestUnitIndex(IReadOnlyList<Unit> unitList, float2 destination)
        {
            (float closestDistance, int closestUnitIndex) = (float.MaxValue, -1);
            for (int unitIndex = 0; unitIndex < unitList.Count; unitIndex++)
            {
                float2 unitPosition = unitList[unitIndex].transform.position.xz();
                float distanceWithDst = distancesq(unitPosition, destination);
                if (distanceWithDst > closestDistance) continue;
                (closestDistance, closestUnitIndex) = (distanceWithDst, unitIndex);
            }
            return closestUnitIndex;
        }
    }
}
