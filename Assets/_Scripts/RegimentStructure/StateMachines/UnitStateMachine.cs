using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class UnitStateMachine : StateMachine<Unit>
    {
        public RegimentStateMachine ParentStateMachine { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ State Machine Override ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public override void TransitionDefaultState()
        {
            TransitionState(UnitState.Default, UnitOrder.Null);
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods (Units are Initialize by their regiment)◈◈◈◈◈◈                          ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public override void Initialize()
        {
            base.Initialize();
            ParentStateMachine = ObjectAttach.RegimentAttach.StateMachine;
        }

        protected override void InitializeStates()
        {
            States = new Dictionary<EStates, State<Unit>>()
            {
                {EStates.None, UnitState.Null},
                {EStates.Idle, new UnitIdleState(ObjectAttach)},
                {EStates.Move, new UnitMoveState(ObjectAttach)},
                {EStates.Fire, new UnitFireState(ObjectAttach)}
            };
            State = EStates.Idle;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Request To Regiment ◈◈◈◈◈◈                                                                     ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnStateRequest(EStates stateRequest)
        {
            ParentStateMachine.OnUnitRequest(this, stateRequest);
        }
    }
}
