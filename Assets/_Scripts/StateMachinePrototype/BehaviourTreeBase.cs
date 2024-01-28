using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Kaizerwald
{
    public class BehaviourTreeBase : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Transform CachedTransform { get; protected set; }
        [field:SerializeField]public EStates State { get; protected set; }
        public Dictionary<EStates, StateBase> States { get; protected set; }
        public Queue<EStates> Interruptions { get; protected set; } = new (2);
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ State Access ◇◇◇◇◇◇                                                                                │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public StateBase CurrentState => States[State];
        public bool IsIdle => State == EStates.Idle;
        public bool IsMoving => State == EStates.Move;
        public bool IsFiring => State == EStates.Fire;
        public bool IsInMelee => State == EStates.Melee;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Transform Access ◇◇◇◇◇◇                                                                            │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public float3 Position => CachedTransform.position;
        public float3 Forward => CachedTransform.forward;
        public float3 Back => -CachedTransform.forward;
        public float3 Right => CachedTransform.right;
        public float3 Left => -CachedTransform.right;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setter ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝ 

        protected virtual void Awake()
        {
            State = EStates.Idle;
            CachedTransform = transform;
        }

        public virtual void OnUpdate()
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
            //Debug.Log($"CurrentState: {State} NEXT State: {nextState}");
            CurrentState.OnExit();
            State = nextState;
            CurrentState.OnEnter();
        }

        public virtual void RequestChangeState(Order order)
        {
            EStates stateOrdered = order.StateOrdered;
            States[stateOrdered].OnSetup(order);
            CurrentState.OnExit();
            State = stateOrdered;
            CurrentState.OnEnter();
        }

        public virtual void ForceChangeState(Order order)
        {
            EStates stateOrdered = order.StateOrdered;
            States[stateOrdered].OnSetup(order);
            CurrentState.OnExit();
            State = stateOrdered;
            CurrentState.OnEnter();
        }
    }
}
