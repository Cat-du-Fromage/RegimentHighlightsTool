using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class StateMachine<T> : MonoBehaviour
    where T : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field: SerializeField] public T ObjectAttach { get; protected set; }
        [field: SerializeField] public EStates State { get; protected set; }
        public Dictionary<EStates, State<T>> States { get; protected set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected virtual void Awake()
        {
            ObjectAttach = GetComponent<T>();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public State<T> CurrentState => States[State];
        public bool IsIdle => State == EStates.Idle;
        public bool IsMoving => State == EStates.Move;
        public bool IsFiring => State == EStates.Fire;

        public virtual void Initialize()
        {
            InitializeStates();
        }
        
        public virtual void OnUpdate()
        {
            CurrentState?.OnStateUpdate();
        }

        public virtual void OnOrderReceived<TOrder>(TOrder order)
        where TOrder : Order
        {
            CurrentState.OnStateExit();
            State = order.StateOrdered;
            CurrentState.OnStateEnter(order);
        }

        public virtual void TransitionState(Order transitionInfo, EStates orderedState = EStates.None)
        {
            CurrentState.OnStateExit();
            State = orderedState == EStates.None ? transitionInfo.StateOrdered : orderedState;
            CurrentState.OnStateEnter(transitionInfo);
        }

        public abstract void TransitionDefaultState();

        protected abstract void InitializeStates();
        /* Exemple type d'implementation
        States = new Dictionary<EStates, State<T>>()
        {
            {EStates.None, new NullState<T>(ObjectAttach)},
            {EStates.Idle, new IdleState<T>(ObjectAttach)},
            {EStates.Move, new MoveState<T>(ObjectAttach)},
            {EStates.Fire, new FireState<T>(ObjectAttach)}
        };
        State = EStates.Idle;
        */
        
    }
}
