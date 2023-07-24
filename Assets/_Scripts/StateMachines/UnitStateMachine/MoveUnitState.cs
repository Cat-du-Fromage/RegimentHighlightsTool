using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    public class MoveUnitState : UnitState
    {
        private const int MarchSpeed = 1;
        private const int RunSpeed = 3;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public float3 LeaderDestination { get; private set; }
        public FormationData FormationDestination { get; private set; }
        
        public float3 UnitDestination { get; private set; }
        
        public bool DestinationReach { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private UnitAnimation UnitAnimation => UnitAttach.Animation;
        private int IndexInRegiment => UnitAttach.IndexInRegiment;

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Move Related ◈◈◈◈◈◈                                                                            ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        private float Speed => UnitAttach.RegimentAttach.RegimentType.Speed;
        public int SpeedModifier { get; private set; }
        private float MoveSpeed => Speed * SpeedModifier;
        public bool IsRunning => SpeedModifier == RunSpeed;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public MoveUnitState(UnitStateMachine linkedUnitStateMachine) : base(linkedUnitStateMachine, EStates.Move)
        {
            
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void OnAbilityTrigger()
        {
            if (IsRunning)
                SetMarching();
            else
                SetRunning();
        }

        public override void SetupState(Order order)
        {
            ResetValuesToDefaults();
            DestinationReach = false;
            MoveOrder moveOrder = (MoveOrder)order;
            FormationDestination = moveOrder.FormationDestination;
            LeaderDestination = moveOrder.LeaderDestination;
        }

        public override void EnterState()
        {
            UnitDestination = FormationDestination.GetUnitRelativePositionToRegiment3D(IndexInRegiment, LeaderDestination);
        }

        public override bool ConditionStateEnter()
        {
            //Debug.Log($"Unit( {UnitAttach.name} ) dest Reach: {DestinationReach}");
            return !DestinationReach;
        }

        public override void UpdateState()
        {
            MoveUnit();
            CheckSwitchState();
        }
        
        public override bool CheckSwitchState()
        {
            //Later: if(Enemy Engage Unit in Melee){}
            return SwitchToIdleState();
        }
        
        public override void ExitState()
        {
            //if value resets here they will try to go back to Move
            //ResetValuesToDefaults();
        }
        
        public override void ResetValuesToDefaults()
        {
            SetMarching();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        // temporaire, pas un vrai pathfinding....
        private void MoveUnit()
        {
            float3 direction = normalizesafe(UnitDestination - Position);
            float3 translation = Time.deltaTime * MoveSpeed * direction;
            UnitTransform.Translate(translation, Space.World);
            UnitTransform.LookAt(Position + FormationDestination.Direction3DForward);
        }
        
        private bool SwitchToIdleState()
        {
            DestinationReach = distancesq(Position, UnitDestination) <= 0.01f;
            if (DestinationReach) LinkedUnitStateMachine.ToDefaultState();
            return DestinationReach;
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
    }
}
