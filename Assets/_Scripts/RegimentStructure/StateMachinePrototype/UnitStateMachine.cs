using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class UnitStateMachine : StateMachine<Unit>
    {
        protected override void Awake()
        {
            base.Awake();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void InitializeStates()
        {
            States = new Dictionary<EStates, State<Unit>>()
            {
                {EStates.Idle, new UnitIdleState(ObjectAttach)},
                {EStates.Move, new UnitMoveState(ObjectAttach)}
            };
            State = EStates.Idle;
        }
    }
}
