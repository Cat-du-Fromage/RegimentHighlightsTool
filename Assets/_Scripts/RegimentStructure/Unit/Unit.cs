using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class Unit : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field: SerializeField] public Regiment RegimentAttach { get; private set; }
        [field: SerializeField] public int IndexInRegiment { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Start()
        {
            InitializeStates();
        }

        private void OnDestroy()
        {
            OnDeath();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void UpdateUnit()
        {
            CurrentState?.OnStateUpdate();
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Unit InitializeUnit(Regiment regiment, int indexInRegiment, int unitLayerIndex)
        {
            RegimentAttach = regiment;
            IndexInRegiment = indexInRegiment;
            gameObject.layer = unitLayerIndex;
            return this;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Unit Update Event ◈◈◈◈◈◈                                                                       ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnDeath()
        {
            if (RegimentAttach == null) return;
            RegimentAttach.OnDeadUnit(this);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE MACHINE ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public EStates currentState;
        public Dictionary<EStates, UnitState> States;

        private UnitState CurrentState => States[currentState];

        public void OnOrderReceived(RegimentOrder order)
        {
            CurrentState.OnStateExit();
            currentState = order.StateOrdered;
            CurrentState.OnOrderEnter(order);
        }

        public void TransitionState(EStates newState)
        {
            CurrentState.OnStateExit();
            currentState = newState;
            CurrentState.OnStateEnter();
        }
        
        private void InitializeStates()
        {
            States = new Dictionary<EStates, UnitState>()
            {
                {EStates.Idle, new UnitIdleState(this)},
                {EStates.Move, new UnitMoveState(this)}
            };
            currentState = EStates.Idle;
        }
        
    }
}