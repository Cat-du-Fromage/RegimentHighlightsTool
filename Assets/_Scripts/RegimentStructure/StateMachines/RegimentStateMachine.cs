using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWald
{
    public sealed partial class RegimentStateMachine : StateMachine<Regiment>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public HashSet<UnitStateMachine> UnitsStateMachine { get; private set; }
        public HashSet<UnitStateMachine> DeadUnitsStateMachine { get; private set; }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ State Machine Override ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public override void TransitionDefaultState()
        {
            ChangeState(Order.Default);
        }

        public override void OnUpdate()
        {
            CleanUpNullUnitsStateMachine();
            base.OnUpdate();
        }

        private void CleanUpNullUnitsStateMachine()
        {
            if (DeadUnitsStateMachine.Count == 0) return;
            UnitsStateMachine.ExceptWith(DeadUnitsStateMachine);
            DeadUnitsStateMachine.Clear();
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        ///Call in <see cref="Regiment"/> Component
        public override void Initialize() 
        {
            base.Initialize();
            UnitsStateMachine = new HashSet<UnitStateMachine>(ObjectAttach.Units.Count);
            foreach (Unit unit in ObjectAttach.Units)
            {
                UnitsStateMachine.Add(unit.StateMachine);
                unit.StateMachine.Initialize();
            }
            //ObjectAttach.Units.ForEach(unit => UnitsStateMachine.Add(unit.StateMachine));
            //foreach (UnitStateMachine unitStateMachine in UnitsStateMachine) { unitStateMachine.Initialize(); }
            
            // ================================================================
            //TEST DEAD UNITS
            DeadUnitsStateMachine = new HashSet<UnitStateMachine>(ObjectAttach.Units.Count);
            // ================================================================
        }

        protected override void InitializeStates()
        {
            States = new Dictionary<EStates, State<Regiment>>()
            {
                {EStates.None, RegimentState.Null},
                {EStates.Idle, new RegimentIdleState(ObjectAttach)},
                {EStates.Move, new RegimentMoveState(ObjectAttach)},
                {EStates.Fire, new RegimentFireState(ObjectAttach)}
            };
            State = EStates.Idle;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Player Orders ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnAbilityTrigger(EStates state)
        {
            States[state].OnAbilityTrigger();
            foreach (UnitStateMachine unitStateMachine in UnitsStateMachine)
            {
                unitStateMachine.States[state].OnAbilityTrigger();
            }
        }
        
        public void OnOrderReceived(Order order)
        {
            State = order.StateOrdered;
            switch (State)
            {
                case EStates.Idle:
                    return;
                case EStates.Move:
                    MoveOrder moveOrder = (MoveOrder)order;
                    CurrentState.OnStateEnter(order);
                    foreach (UnitStateMachine unitStateMachine in UnitsStateMachine)
                    {
                        unitStateMachine.OnOrderReceived(moveOrder);
                    }
                    return;
                case EStates.Fire:
                    return;
                default:
                    return;
            }
        }
        
        /*
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Units Request ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnUnitRequest(UnitStateMachine unitStateMachine, EStates stateRequest)
        {
            switch (stateRequest)
            {
                case EStates.Fire: //We need a target
                    RegimentFireState fireState = (RegimentFireState)States[EStates.Fire];
                    fireState.OnUnitRequest(unitStateMachine);
                    return;
                default:
                    return;
            }
        }
        */
    }
}
