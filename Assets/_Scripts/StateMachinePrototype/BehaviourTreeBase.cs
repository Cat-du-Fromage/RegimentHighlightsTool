using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace KaizerWald
{
    public class BehaviourTreeBase : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private Transform cachedTransform;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public EStates State { get; private set; }
        public Dictionary<EStates, StateBase> States { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public StateBase CurrentState => States[State];
        
        public float3 Position => cachedTransform.position;
        public float3 Forward => cachedTransform.forward;
        public float3 Back => -cachedTransform.forward;
        public float3 Right => cachedTransform.right;
        public float3 Left => -cachedTransform.right;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝ 

        protected virtual void Awake()
        {
            cachedTransform = transform;
        }

        protected virtual void Update()
        {
            TryChangeState();
            CurrentState.OnUpdate();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void TryChangeState()
        {
            EStates nextState = CurrentState.ShouldExit();
            if (nextState == EStates.None || nextState == CurrentState.StateIdentity) return;
            CurrentState.OnExit();
            State = nextState;
            CurrentState.OnEnter();
        }
    }
}
