using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KaizerWald.UnityMathematicsExtension;
using static KaizerWald.CSharpContainerUtils;

namespace KaizerWald
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
    //║ ◈◈◈◈◈◈ Getters ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public float Speed => RegimentAttach.RegimentType.Speed;
        public float MoveSpeed => Speed * SpeedModifier;
        public bool IsRunning => SpeedModifier == RunSpeed;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setters ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        private void SetMarching() => SpeedModifier = MarchSpeed;
        private void SetRunning() => SpeedModifier = RunSpeed;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment_MoveState(RegimentBehaviourTree behaviourTree, Blackboard blackBoard) : base(behaviourTree,blackBoard,EStates.Move)
        {
            MarchSpeed = 1;
            RunSpeed = 3;
        }

        public override void OnSetup(Order order)
        {
            LeaderReachDestination = false;
            MoveOrder moveOrder = (MoveOrder)order;
            RegimentBlackboard.SetDestination(moveOrder.LeaderDestination);
            RegimentBlackboard.SetDestinationFormation(moveOrder.FormationDestination);
            RegimentBlackboard.ResetTarget();
            if (moveOrder.MoveType == EMoveType.Run)
            {
                SetRunning();
            }
            else
            {
                SetMarching();
            }
        }

        public override void OnEnter()
        {
            UpdateDestinationReach();
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
    //║ ◈◈◈◈◈◈ On Enter ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        private void AssignIndexToUnits(in FormationData formation)
        {
            NativeArray<float2> destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination.xz);
            List<Unit> tempUnitList = new(RegimentAttach.Units);
            SortedSet<KeyValuePair<int, float>> distances = new(GetKeyValuePairComparer<int, float>());
            for(int i = 0; i < destinations.Length; i++)
            {
                GatherUnitsDistance(distances, tempUnitList, destinations[i]);
                Unit unitToRemove = tempUnitList[distances.Min.Key];
                unitToRemove.SetIndexInRegiment(i);
                tempUnitList.Remove(unitToRemove);
                distances.Clear();
            }
        }
        
        private void GatherUnitsDistance(ISet<KeyValuePair<int, float>> distances, IReadOnlyList<Unit> unitList, float2 destination)
        {
            for(int unitIndex = 0; unitIndex < unitList.Count; unitIndex++)
            {
                float3 unitPosition = unitList[unitIndex].transform.position;
                float distanceWithDst = distancesq(unitPosition.xz, destination);
                KeyValuePair<int, float> pair = new (unitIndex, distanceWithDst);
                distances.Add(pair);
            }
        }
    }
}
