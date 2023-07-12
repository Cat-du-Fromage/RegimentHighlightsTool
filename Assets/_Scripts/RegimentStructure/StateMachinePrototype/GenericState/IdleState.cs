using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace KaizerWald
{
    public class IdleState<T> : State<T>
    where T : MonoBehaviour
    {
        public IdleState(T objectAttach) : base(objectAttach)
        {
        }

        public override void OnAbilityTrigger() { return; }

        public override void OnStateEnter() { return; }
        public override void OnOrderEnter(RegimentOrder order) { return; }
        public override void OnStateUpdate() { return; }
        public override bool OnTransitionCheck() { return false; }
        public override void OnStateExit() { return; }
    }
}