using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentState
    {
        public static readonly EStates Default = EStates.Idle;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public EStates StateIdentity { get; private set; }
        protected RegimentStateMachine LinkedRegimentStateMachine { get; private set; }
        protected Regiment RegimentAttach { get; private set; }
        protected Transform RegimentTransform { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public virtual bool IsNull => false;
        
        public float3 Position => RegimentTransform.position;
        public float3 Forward => RegimentTransform.forward;
        public float3 Back => -RegimentTransform.forward;
        public float3 Right => RegimentTransform.right;
        public float3 Left => -RegimentTransform.right;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected RegimentState(RegimentStateMachine regimentStateMachine, EStates stateIdentity)
        {
            StateIdentity = stateIdentity;
            LinkedRegimentStateMachine = regimentStateMachine;
            RegimentAttach = regimentStateMachine.Regiment;
            RegimentTransform = regimentStateMachine.transform;
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Base Methods ◈◈◈◈◈◈                                                                            ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public virtual void OnAbilityTrigger() { return; }
        public virtual bool ConditionStateEnter() { return true; }
        public virtual void SetupState(Order order) { return; }
        public virtual void EnterState() { return; }

        public virtual void UpdateState() { return; }
        public virtual bool CheckSwitchState() { return false; }
        public virtual void ExitState() { return; }
        public virtual void ResetValuesToDefaults() { return; }
    }
}
