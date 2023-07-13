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
        public List<UnitStateMachine> UnitsStateMachine { get; private set; }
        
        
        

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public override void Initialize()
        {
            base.Initialize();
            UnitsStateMachine = new List<UnitStateMachine>(ObjectAttach.Units.Count);
            ObjectAttach.Units.ForEach(unit => UnitsStateMachine.Add(unit.StateMachine));
        }

        protected override void InitializeStates()
        {
            States = new Dictionary<EStates, State<Regiment>>()
            {
                {EStates.Idle, new RegimentIdleState(ObjectAttach)},
                {EStates.Move, new RegimentMoveState(ObjectAttach)}
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
                    CurrentState.OnOrderEnter(order);
                    foreach (UnitStateMachine unitStateMachine in UnitsStateMachine)
                    {
                        unitStateMachine.OnOrderReceived(order);
                    }
                    //foreach (Unit unit in ObjectAttach.Units) unit.StateMachine.OnOrderReceived(order);
                    return;
                default:
                    return;
            }
        }
    }
}
