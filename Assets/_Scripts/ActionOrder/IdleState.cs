using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    /// <summary>
    /// Transition Possible
    /// -Move
    /// -Attack
    /// </summary>
    public class IdleState : RegimentState
    {
        public override void HandleInput()
        {
            //InputAction: FireAtWill(ON/OFF)
        }

        public override IRegimentState UpdateState(Regiment regiment)
        {
            throw new System.NotImplementedException();
        }

        public override void OnEnter(Regiment regiment)
        {
            throw new System.NotImplementedException();
        }

        public override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public override void OnExit(Regiment regiment)
        {
            throw new System.NotImplementedException();
        }
    }
}
