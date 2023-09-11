using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class Unit_IdleState : UnitStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Unit_IdleState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Idle)
        {
            
        }

        public override void OnSetup(Order order)
        {
            return;
        }

        public override void OnEnter()
        {
            UnitAnimation.SetIdle();
        }

        public override void OnUpdate()
        {
            //Rien?
        }

        public override void OnExit()
        {
            //Rien?
        }

        public override EStates ShouldExit()
        {
            return TryReturnToRegimentState(out EStates nextState) ? nextState : StateIdentity;
        }
        
        private bool TryReturnToRegimentState(out EStates nextState)
        {
            nextState = StateIdentity != RegimentState ? RegimentState : StateIdentity;
            return true;
        }
    }
}
