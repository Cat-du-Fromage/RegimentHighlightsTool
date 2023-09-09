using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class Unit_RangeAttackState : UnitStateBase
    {
        public Unit_RangeAttackState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Fire)
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
