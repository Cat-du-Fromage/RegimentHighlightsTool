using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class UnitIdleState : UnitState
    {
        public UnitIdleState(Unit unit) : base(unit)
        {
        }

        public override void OnStateEnter()
        {
            return;
        }

        public override void OnOrderEnter(RegimentOrder order)
        {
            return;
        }

        public override void OnStateUpdate()
        {
            return;
        }

        public override bool OnTransitionCheck()
        {
            return false;
        }

        public override void OnStateExit()
        {
            return;
        }
    }
}
