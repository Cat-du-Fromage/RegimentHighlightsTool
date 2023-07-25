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
        //[field: SerializeField] public int IndexInRegiment { get; private set; }
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
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝


        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Interface Methods ◈◈◈◈◈◈                                                                       ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        //DEBUG
        [field:SerializeField]public int IndexInRegimentDebug { get; private set; }
        //DEBUG
        public UnitMatrixElement FormationMatrix { get; private set; }
        public int IndexInRegiment => FormationMatrix.IndexInRegiment;

        public void InitializeFormationMatrix(RegimentFormationMatrix regimentFormationMatrix, int index)
        {
            //------------------------------------------
            //DEBUG
            IndexInRegimentDebug = index;
            //------------------------------------------
            FormationMatrix = new UnitMatrixElement(regimentFormationMatrix, index);
        }
        
        public void SetIndexInRegiment(int index)
        {
            FormationMatrix.SetIndexInRegiment(index);
            //------------------------------------------
            //DEBUG
            IndexInRegimentDebug = IndexInRegimentDebug;
            //------------------------------------------
        }

        public void UpdateUnit()
        {
            StateMachine.OnUpdate();
        }

        //------------------------------------------
        //DEBUG
        private void Update()
        {
            if (IndexInRegimentDebug == FormationMatrix.IndexInRegiment) return;
            IndexInRegimentDebug = FormationMatrix.IndexInRegiment;
        }
        //------------------------------------------
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods (Units are Initialize by their regiment) ◈◈◈◈◈◈                         ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Unit Initialize(Regiment regiment/*, int indexInRegiment*/, int unitLayerIndex)
        {
            InitializeProperties(regiment/*, indexInRegiment*/, unitLayerIndex);
            return this;
        }

        private void InitializeProperties(Regiment regiment/*, int indexInRegiment*/, int unitLayerIndex)
        {
            RegimentAttach = regiment;
            //IndexInRegiment = indexInRegiment;
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