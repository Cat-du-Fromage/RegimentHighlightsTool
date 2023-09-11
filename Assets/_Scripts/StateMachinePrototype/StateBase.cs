using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace KaizerWald
{
    public abstract class StateBase
    {
        public EStates StateIdentity { get; private set; }

        protected StateBase(EStates stateIdentity)
        {
            StateIdentity = stateIdentity;
        }
        
        //Specific to Player Order
        public abstract void OnSetup(Order order);
        
        public abstract void OnEnter();
        
        public abstract void OnUpdate();
        
        public abstract void OnExit();
        
        public abstract EStates ShouldExit();
        
        protected virtual EStates CheckInterruptionExit() => EStates.None;
        
        protected virtual EStates CheckSequenceExit() => EStates.None;
    }
}
