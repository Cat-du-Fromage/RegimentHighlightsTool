using System.Collections;
using System.Collections.Generic;

namespace KaizerWald
{
    //Package sent to create MoveState

    public class MoveState : RegimentState
    {
        private IRegimentState IdleState;
        public override void HandleInput()
        {
            //InputAction: Run/March
        }

        //End: Move -> Idle
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
