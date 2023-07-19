using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWald
{
    public partial class RegimentStateMachine : StateMachine<Regiment>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public HashSet<UnitStateMachine> UnitsStateMachine { get; private set; }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ State Machine Override ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public override void TransitionDefaultState()
        {
            TransitionState(RegimentOrder.Null, RegimentState.Default);
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        ///Call in <see cref="Regiment"/> Component
        public override void Initialize() 
        {
            base.Initialize();
            UnitsStateMachine = new HashSet<UnitStateMachine>(ObjectAttach.Units.Count);
            ObjectAttach.Units.ForEach(unit => UnitsStateMachine.Add(unit.StateMachine));
            foreach (UnitStateMachine unitStateMachine in UnitsStateMachine)
            {
                unitStateMachine.Initialize();
            }
            //UnitsStateMachine.ForEach(machine => machine.Initialize());
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
        
        public void OnMoveOrderReceived(RegimentOrder order)
        {
            State = order.StateOrdered;
            switch (State)
            {
                case EStates.Idle:
                    return;
                case EStates.Move:
                    RegimentMoveOrder regimentMoveOrder = (RegimentMoveOrder)order;
                    CurrentState.OnStateEnter(order);
                    foreach (UnitStateMachine unitStateMachine in UnitsStateMachine)
                    {
                        UnitMoveOrder unitOrder = new (unitStateMachine.ObjectAttach.IndexInRegiment, regimentMoveOrder);
                        unitStateMachine.OnOrderReceived(unitOrder);
                    }
                    //foreach (Unit unit in ObjectAttach.Units) unit.StateMachine.OnOrderReceived(order);
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
