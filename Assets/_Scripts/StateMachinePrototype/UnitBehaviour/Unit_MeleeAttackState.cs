using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class Unit_MeleeAttackState : UnitStateBase
    {
        public Unit_MeleeAttackState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Melee)
        {
        }

        public override void OnSetup(Order order)
        {
            //
        }

        public override void OnEnter()
        {
            //
        }

        public override void OnUpdate()
        {
            //
        }

        public override void OnExit()
        {
            //
        }

        public override EStates ShouldExit()
        {
            return StateIdentity;
        }
    }
}
