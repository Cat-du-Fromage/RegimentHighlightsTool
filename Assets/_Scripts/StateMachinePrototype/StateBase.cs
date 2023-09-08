using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace KaizerWald
{
    public abstract class StateBase
    {
        public EStates StateIdentity { get; private set; }
        
        public List<StateBase> Sequences;
        
        public SortedList<int,StateBase> Interruptions; // both external and Order

        protected StateBase(EStates stateIdentity)
        {
            StateIdentity = stateIdentity;
        }
        
        public abstract void OnEnter();
        
        public abstract void OnUpdate();
        
        public abstract void OnExit();
        
        public abstract EStates ShouldExit();
        
        protected virtual EStates CheckInterruptionExit() => EStates.None;
        
        protected virtual EStates CheckSequenceExit() => EStates.None;
    }
}
