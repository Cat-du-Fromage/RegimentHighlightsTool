using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentBehaviourTree : BehaviourTreeBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Blackboard RegimentBlackboard { get; private set; }
        public Regiment RegimentAttach { get; private set; }
        public HashSet<UnitBehaviourTree> UnitsBehaviourTrees { get; private set; }
        public HashSet<UnitBehaviourTree> DeadUnitsBehaviourTrees { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝ 
        protected override void Awake()
        {
            base.Awake();
            Initializations();
        }

        public override void OnUpdate()
        {
            //if (RegimentAttach.IsSelected) Debug.Log($"BEFORE update BT: {State}");
            CleanUpNullUnitsStateMachine();
            RegimentBlackboard.UpdateInformation();
            base.OnUpdate();
            //if (RegimentAttach.IsSelected) Debug.Log($"AFTER update BT: {State}");
        }

        private void OnDestroy()
        {
            foreach ((EStates _, StateBase state) in States)
            {
                state.OnDestroy();
            }
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void CleanUpNullUnitsStateMachine()
        {
            if (DeadUnitsBehaviourTrees.Count == 0) return;
            UnitsBehaviourTrees.ExceptWith(DeadUnitsBehaviourTrees);
            DeadUnitsBehaviourTrees.Clear();
        }

        public void OnOrderReceived(Order order)
        {
            //UTILISER LES CONDITION D'ENTREE DES REGIMENT STATE! Comme fait pour unit fire State!
            Order processedOrder = RegimentBlackboard.OnOrder(order);
            RequestChangeState(processedOrder);
        }
        
        public override void RequestChangeState(Order order)
        {
            base.RequestChangeState(order);
            //Propagate Order to Units
            foreach (UnitBehaviourTree unitBehaviourTree in UnitsBehaviourTrees)
            {
                unitBehaviourTree.RequestChangeState(order);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Initializations()
        {
            if (!TryGetComponent(out Regiment regimentAttach))
            {
                Debug.LogError("No Regiment Component Attach with the BehaviourTree");
            }
            else
            {
                RegimentAttach = regimentAttach;
                RegimentBlackboard = new Blackboard(RegimentAttach);
                InitializeStates();
            
                UnitsBehaviourTrees = new HashSet<UnitBehaviourTree>(RegimentAttach.Units.Count);
                DeadUnitsBehaviourTrees = new HashSet<UnitBehaviourTree>(RegimentAttach.Units.Count);
                foreach (Unit unit in RegimentAttach.Units)
                {
                    UnitBehaviourTree unitBt = unit.SetBehaviourTree(this);
                    UnitsBehaviourTrees.Add(unitBt);
                }
            }
        }
        
        private void InitializeStates()
        {
            States = new Dictionary<EStates, StateBase>()
            {
                {EStates.Idle, new Regiment_IdleState(this, RegimentBlackboard)},
                {EStates.Move, new Regiment_MoveState(this, RegimentBlackboard)},
                {EStates.Fire, new Regiment_RangeAttackState(this, RegimentBlackboard)},
                //{EStates.Melee, new ChaseRegimentState(this)},
            };
            
        }
    }
}
