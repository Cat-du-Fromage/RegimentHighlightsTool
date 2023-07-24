using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class IdleUnitState : UnitState
    {
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private EStates UnitState => LinkedUnitStateMachine.State;
        private EStates RegimentState => LinkedUnitStateMachine.ParentStateMachine.State;
        private UnitAnimation UnitAnimation => UnitAttach.Animation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public IdleUnitState(UnitStateMachine linkedUnitStateMachine) : base(linkedUnitStateMachine, EStates.Idle)
        {
            
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void EnterState()
        {
            UnitAnimation.SetIdle();
        }

        public override void UpdateState()
        {
            CheckSwitchState();
        }

        public override bool CheckSwitchState()
        {
            //Check either unit has same state than regiment OR condition to enter new Unit state are not met
            if(UnitState == RegimentState || !LinkedUnitStateMachine.States[RegimentState].ConditionStateEnter()) return false;
            LinkedUnitStateMachine.TryEnterState(RegimentState);
            return true;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
    }
}
