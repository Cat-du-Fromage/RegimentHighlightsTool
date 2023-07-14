using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace KaizerWald
{
    public abstract class State<T>
        where T : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected readonly T ObjectAttach;
        protected readonly Transform ObjectTransform;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected State(T objectAttach)
        {
            ObjectAttach = objectAttach;
            ObjectTransform = objectAttach.transform;
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //void EventTransition(T params)...(construction sur les states car les paramètres diffère selon chaque état)
        public virtual void OnAbilityTrigger() { return; }
        public virtual void OnStateEnter() { return; }
        public virtual void OnOrderEnter(RegimentOrder order) { return; }
        public virtual void OnStateUpdate() { return; }
        public virtual bool OnTransitionCheck() { return false; }
        public virtual void OnStateExit() { return; }
        public virtual void ResetDefaultValues() { return; }
    }
}