using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class Unit_IdleState : UnitStateBase
    {
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
            EStates nextState = CheckInterruptionExit();
            //Si Unité engagé en Melee => return EState.MeleeAttack
            if (nextState != EStates.None) return nextState;
            
            
            //Si Unité engagé en Melee => return EState.MeleeAttack
            nextState = CheckSequenceExit();
            if (nextState != EStates.None) return nextState;

            //return to regiment state or keep the same
            nextState = GetRegimentState();
            return nextState != StateIdentity ? nextState : StateIdentity;
        }

        protected override EStates CheckInterruptionExit()
        {
            //Si Unité engagé en Melee => return EState.MeleeAttack
            //Si Ordre Reçu:
            // 1) Movement
            // 2) Melee
            return EStates.None;
        }

        private void CheckRegimentState()
        {
            // Move ? difference btw order and pregress state
            // Melee ?
        }
    }
}
