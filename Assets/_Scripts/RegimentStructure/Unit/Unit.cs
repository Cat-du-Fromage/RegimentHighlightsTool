using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public partial class Unit : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private Rigidbody unitRigidBody;
        private Collider unitCollider;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field: SerializeField] public int IndexInRegiment { get; private set; }
        [field: SerializeField] public Regiment RegimentAttach { get; private set; }
        [field: SerializeField] public UnitAnimation Animation { get; private set; }
        [field: SerializeField] public UnitStateMachine StateMachine { get; private set; }
        
        [field: SerializeField] public bool IsDead { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Awake()
        {
            unitRigidBody = GetComponent<Rigidbody>();
            unitCollider = GetComponent<Collider>();
            Animation = GetComponent<UnitAnimation>();
            StateMachine = this.GetOrAddComponent<UnitStateMachine>();
        }
/*
        private void OnDestroy()
        {
            OnDeath();
        }
*/
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void SetIndexInRegiment(int index) => IndexInRegiment = index;

        public void UpdateUnit()
        {
            StateMachine.OnUpdate();
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods (Units are Initialize by their regiment) ◈◈◈◈◈◈                         ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Unit Initialize(Regiment regiment, int indexInRegiment, int unitLayerIndex)
        {
            InitializeProperties(regiment, indexInRegiment, unitLayerIndex);
            return this;
        }

        private void InitializeProperties(Regiment regiment, int indexInRegiment, int unitLayerIndex)
        {
            RegimentAttach = regiment;
            IndexInRegiment = indexInRegiment;
            gameObject.layer = unitLayerIndex;
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Unit Event ◈◈◈◈◈◈                                                                              ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        //CAREFULLE EVENT RECEIVED ON FIXED UPDATE => BEFORE UPDATE
        //Death trigger on regiment because of update order related issue
        public void ConfirmDeathByRegiment(Regiment regimentOwner)
        {
            if (regimentOwner.RegimentID != RegimentAttach.RegimentID) return;
            if (IsDead) return;
            unitCollider.enabled = false;
            unitRigidBody.Sleep();
            Animation.SetDead();
            IsDead = true;
        }
        
        public void TriggerDeath()
        {
            RegimentAttach.OnDeadUnit(this);
        }
    }
}