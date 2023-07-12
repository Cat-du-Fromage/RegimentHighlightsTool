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
        protected T ObjectAttach;
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

        public virtual void Initialize()
        {
            InitializeStates();
        }
        
        public virtual void OnUpdate()
        {
            CurrentState?.OnStateUpdate();
        }

        public virtual void OnOrderReceived(RegimentOrder order)
        {
            CurrentState.OnStateExit();
            State = order.StateOrdered;
            CurrentState.OnOrderEnter(order);
        }

        public virtual void TransitionState(EStates newState)
        {
            CurrentState.OnStateExit();
            State = newState;
            CurrentState.OnStateEnter();
        }

        protected abstract void InitializeStates();
        /* Exemple type d'implementation
        States = new Dictionary<EStates, State<T>>()
        {
            {EStates.Idle, new IdleState<T>(ObjectAttach)},
            {EStates.Move, new MoveState<T>(ObjectAttach)}
        };
        State = EStates.Idle;
        */
        
    }
}
