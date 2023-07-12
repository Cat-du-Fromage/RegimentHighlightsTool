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

        [field: SerializeField] public UnitStateMachine StateMachine { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Awake()
        {
            StateMachine = this.GetOrAddComponent<UnitStateMachine>();
        }

        private void OnDestroy()
        {
            OnDeath();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void SetIndexInRegiment(int index) => IndexInRegiment = index;

        public void UpdateUnit()
        {
            StateMachine.OnUpdate();
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Unit Initialize(Regiment regiment, int indexInRegiment, int unitLayerIndex)
        {
            InitializeProperties(regiment, indexInRegiment, unitLayerIndex);
            InitializeStateMachine();
            return this;
        }

        private void InitializeProperties(Regiment regiment, int indexInRegiment, int unitLayerIndex)
        {
            RegimentAttach = regiment;
            IndexInRegiment = indexInRegiment;
            gameObject.layer = unitLayerIndex;
        }

        private void InitializeStateMachine()
        {
            StateMachine = this.GetOrAddComponent<UnitStateMachine>();
            StateMachine.Initialize();
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Unit Update Event ◈◈◈◈◈◈                                                                       ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnDeath()
        {
            if (RegimentAttach == null) return;
            RegimentAttach.OnDeadUnit(this);
        }
    }
}