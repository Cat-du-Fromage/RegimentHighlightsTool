using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kaizerwald
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
            //if (FireExit()) return EStates.Fire;
            return TryReturnToRegimentState();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public EStates TryReturnToRegimentState()
        {
            if (StateIdentity == RegimentState) return StateIdentity;
            return BehaviourTree.States[RegimentState].ConditionEnter() ? RegimentState : StateIdentity;
        }
    }
}
