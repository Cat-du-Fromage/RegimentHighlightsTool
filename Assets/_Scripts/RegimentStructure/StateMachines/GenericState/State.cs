using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace KaizerWald
{
    public abstract class State<T>
    where T : MonoBehaviour
    {
        protected const EStates Default = EStates.Idle;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public readonly EStates StateIdentity;
        
        protected T ObjectAttach{ get; private set; }
        protected readonly StateMachine<T> LinkedStateMachine;
        protected readonly Transform ObjectTransform;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public EStates NextState { get; private set; }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected State(T objectAttach, EStates stateIdentity)
        {
            ObjectAttach = objectAttach;
            StateIdentity = stateIdentity;
            ObjectTransform = objectAttach == null ? null : objectAttach.transform;
            LinkedStateMachine = objectAttach == null ? null :objectAttach.GetComponent<StateMachine<T>>();
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public virtual bool IsNull => false;
        public float3 Position => ObjectTransform.position;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Base Methods ◈◈◈◈◈◈                                                                            ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        //void EventTransition(T params)...(construction sur les states car les paramètres diffère selon chaque état)
        public virtual void OnAbilityTrigger() { return; }
        public virtual void OnStateEnter(Order order) { return; }
        public virtual void OnStateUpdate() { return; }
        public virtual bool OnTransitionCheck() { return false; }
        public virtual void OnStateExit() { return; }
        public virtual void ResetDefaultValues() { return; }
    }
}