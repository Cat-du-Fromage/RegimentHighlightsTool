using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class FireState<T> : State<T>
    where T : MonoBehaviour
    {
        public T Target { get; protected set; }
        
        public FireState(T objectAttach) : base(objectAttach)
        {
            Target = null;
        }

        public override void OnAbilityTrigger() { return;}
        public override void OnStateEnter() { return; }
        public override void OnOrderEnter(RegimentOrder order) { return; }
        public override void OnStateUpdate() { return; }
        public override bool OnTransitionCheck() { return false; }
        public override void OnStateExit() { return; }
    }
}
