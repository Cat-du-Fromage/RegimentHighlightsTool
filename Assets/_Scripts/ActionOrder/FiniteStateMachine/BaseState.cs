using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class BaseState
    {
        public string Name;

        protected StateMachine StateMachine;
    
        public BaseState(string name, StateMachine stateMachine)
        {
            Name = name;
            StateMachine = stateMachine;
        }

        public virtual void OnEnter() {}
        public virtual void OnUpdate() {}
        public virtual void OnFixedUpdate() {}
        public virtual void OnExit() {}
    }
}
