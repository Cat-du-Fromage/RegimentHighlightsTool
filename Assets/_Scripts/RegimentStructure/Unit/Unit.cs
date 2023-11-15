using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
//------------------------------------------------------------------------------------------------------------------------------
    //TODO : "OnDeath" Desactiver la collision APRES la fin de l'animation
//------------------------------------------------------------------------------------------------------------------------------
    public partial class Unit : MonoBehaviour, IComparable<Unit>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private Rigidbody unitRigidBody;
        private Collider unitCollider;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public UnitMatrixElement FormationMatrix { get; private set; }
        [field: SerializeField] public Regiment RegimentAttach { get; private set; }
        [field: SerializeField] public UnitAnimation Animation { get; private set; }
        [field: SerializeField] public UnitBehaviourTree BehaviourTree { get; private set; }
        [field: SerializeField] public bool IsDead { get; private set; }
        
//------------------------------------------------------------------------------------------------------------------------------
//DEBUG: TO BE REMOVED
        [field:SerializeField]public int IndexInRegimentDebug { get; private set; }
//------------------------------------------------------------------------------------------------------------------------------
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public int IndexInRegiment => FormationMatrix.IndexInRegiment;
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Getters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        public UnitBehaviourTree SetBehaviourTree(RegimentBehaviourTree regimentBt) => BehaviourTree = this.GetOrAddComponent<UnitBehaviourTree>();
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Awake()
        {
            unitRigidBody = GetComponent<Rigidbody>();
            unitCollider = GetComponent<Collider>();
            Animation = GetComponent<UnitAnimation>();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Interface Methods ◈◈◈◈◈◈                                                                            ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void InitializeFormationMatrix(RegimentFormationMatrix regimentFormationMatrix, int index)
        {
//------------------------------------------------------------------------------------------------------------------------------
//DEBUG: TO BE REMOVED
            IndexInRegimentDebug = index;
//------------------------------------------------------------------------------------------------------------------------------
            FormationMatrix = new UnitMatrixElement(regimentFormationMatrix, index);
        }
        
        public void SetIndexInRegiment(int index)
        {
            FormationMatrix.SetIndexInRegiment(index);
//------------------------------------------------------------------------------------------------------------------------------
//DEBUG: TO BE REMOVED
            IndexInRegimentDebug = IndexInRegimentDebug;
//------------------------------------------------------------------------------------------------------------------------------
        }

        public void UpdateUnit()
        {
            BehaviourTree.OnUpdate();
        }

//------------------------------------------------------------------------------------------------------------------------------
//DEBUG: TO BE REMOVED
        private void Update()
        {
            if (IndexInRegimentDebug == FormationMatrix.IndexInRegiment) return;
            IndexInRegimentDebug = FormationMatrix.IndexInRegiment;
        }
//------------------------------------------------------------------------------------------------------------------------------
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods (Units are Initialize by their regiment) ◈◈◈◈◈◈                              ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Unit Initialize(Regiment regiment, int unitLayerIndex)
        {
            InitializeProperties(regiment, unitLayerIndex);
            return this;
        }

        private void InitializeProperties(Regiment regiment, int unitLayerIndex)
        {
            RegimentAttach = regiment;
            gameObject.layer = unitLayerIndex;
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Unit Event ◈◈◈◈◈◈                                                                                   ║
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

        public int CompareTo(Unit other)
        {
            if (other == null) return -1;
            return this.FormationMatrix.CompareTo(other.FormationMatrix);
        }
    }
}