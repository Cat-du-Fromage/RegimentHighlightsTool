using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    public class Unit_MoveState : UnitStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public const EStates DefaultNextState = EStates.Idle;
        private readonly int MarchSpeed;
        private readonly int RunSpeed;
        private Dictionary<Regiment, float> EngagementInvulnerability = new (4);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool UnitReachDestination { get; private set; }
        public float3 LeaderDestination { get; private set; }
        public FormationData FormationDestination { get; private set; }
        public float3 UnitDestination { get; private set; }
        public int SpeedModifier { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Getters ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private EMoveType MoveType => IsRunning ? EMoveType.Run : EMoveType.March;
        public float Speed => ParentRegiment.RegimentType.Speed;
        public float MoveSpeed => Speed * SpeedModifier;
        public bool IsRunning => SpeedModifier == RunSpeed;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Unit_MoveState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Move)
        {
            MarchSpeed = 1;
            RunSpeed = 3;
            SpeedModifier = RunSpeed;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void OnSetup(Order order)
        {
            UnitReachDestination = false;
            MoveOrder moveOrder = (MoveOrder)order;
            SpeedModifier = moveOrder.MoveType == EMoveType.Run ? RunSpeed : MarchSpeed;
            //Seems of
            FormationDestination = moveOrder.FormationDestination;
            LeaderDestination = moveOrder.LeaderDestination;
            //UnitDestination = FormationDestination.GetUnitRelativePositionToRegiment3D(IndexInRegiment, LeaderDestination);
        }

        public override void OnEnter()
        {
            UnitDestination = FormationDestination.GetUnitRelativePositionToRegiment3D(IndexInRegiment, RegimentBlackboard.Destination);
            UpdateDestinationReach();
            SetAnimationSpeed(MoveType);
            AdaptSpeed();
        }

        public override void OnUpdate()
        {
            
            if (UnitReachDestination || UnitAttach.IsDead) return;
            if (!LeaderDestination.approximately(ParentRegiment.Highlight.DestinationLeaderPosition))
            {
                LeaderDestination = ParentRegiment.Highlight.DestinationLeaderPosition;
                /*
                // ======================================================================
                bool destFormation = FormationDestination == default;
                Debug.Log($"FormationDestination null ? {destFormation}");
                
                bool parentNull = ParentRegiment == null;
                Debug.Log($"parentNull ? {parentNull}");
                if (!parentNull)
                {
                    bool highlightNull = ParentRegiment.Highlight == null;
                    Debug.Log($"parentNull ? {highlightNull}");
                    if (!highlightNull)
                    {
                        bool destinationFormationNull = ParentRegiment.Highlight.DestinationFormation == null;
                        Debug.Log($"ParentRegiment.Highlight.DestinationFormation null ? {destinationFormationNull}");
                    }
                }
                // ======================================================================
                */
                FormationDestination = ParentRegiment.Highlight.DestinationFormation;
            }
            //UnitDestination = FormationDestination.GetUnitRelativePositionToRegiment3D(IndexInRegiment, RegimentBlackboard.Destination);
            MoveUnit();
        }

        public override void OnExit()
        {
            UnitReachDestination = false;
        }

        public override EStates ShouldExit()
        {
            UpdateDestinationReach();
            return TryReturnToRegimentState();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void UpdateDestinationReach()
        {
            if (UnitReachDestination) return;
            UnitReachDestination = distancesq(Position, UnitDestination) <= 0.0125f;
        }
        
        private EStates TryReturnToRegimentState()
        {
            if (StateIdentity == RegimentState || !UnitReachDestination) return StateIdentity;
            bool canEnterNextState = BehaviourTree.States[RegimentState].ConditionEnter();
            EStates nextState = canEnterNextState ? RegimentState : DefaultNextState;
            return nextState;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ State Logic ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void SetAnimationSpeed(EMoveType moveType)
        {
            if (moveType == EMoveType.Run)
                SetRunning();
            else
                SetMarching();
        }
    
        private void SetMarching()
        {
            SpeedModifier = MarchSpeed;
            UnitAnimation.SetMarching();
        }

        private void SetRunning()
        {
            SpeedModifier = RunSpeed;
            UnitAnimation.SetRunning();
        }

        private void MoveUnit()
        {
            AdaptSpeed();
            float3 direction = normalizesafe(UnitDestination - Position);
            float3 translation = Time.deltaTime * MoveSpeed * direction;
            UnitTransform.Translate(translation, Space.World);
            UnitTransform.LookAt(Position + FormationDestination.Direction3DForward);
        }

        private void AdaptSpeed()
        {
            if (!IsRunning || distancesq(Position, UnitDestination) > 0.125f) return; 
            SetMarching();
        }
    }
}
