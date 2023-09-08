using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class Unit_IdleState : StateBase
    {
        public Unit_IdleState() : base(EStates.Idle)
        {
            Sequences = new List<StateBase>();
            Interruptions = new SortedList<int, StateBase>();
        }

        public override void OnEnter()
        {
            //Animation
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
            if (nextState != EStates.None)
            {
                return nextState;
            }
            
            //Si Unité engagé en Melee => return EState.MeleeAttack
            nextState = CheckSequenceExit();
            if (nextState != EStates.None)
            {
                return nextState;
            }
            
            return StateIdentity;
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
