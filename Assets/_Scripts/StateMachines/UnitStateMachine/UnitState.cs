using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public class UnitState
    {
        public static readonly EStates Default = EStates.Idle;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public EStates StateIdentity { get; private set; }
        protected UnitStateMachine LinkedUnitStateMachine { get; private set; }
        protected Unit UnitAttach { get; private set; }
        protected Transform UnitTransform { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public virtual bool IsNull => false;
        
        public float3 Position => UnitTransform.position;
        public float3 Forward => UnitTransform.forward;
        public float3 Back => -UnitTransform.forward;
        public float3 Right => UnitTransform.right;
        public float3 Left => -UnitTransform.right;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected UnitState(UnitStateMachine linkedUnitStateMachine, EStates stateIdentity)
        {
            StateIdentity = stateIdentity;
            LinkedUnitStateMachine = linkedUnitStateMachine;
            UnitAttach = linkedUnitStateMachine.Unit;
            UnitTransform = linkedUnitStateMachine.transform;
        }
        
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
