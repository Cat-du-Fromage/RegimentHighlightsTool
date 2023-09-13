using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentBehaviourTree : BehaviourTreeBase
    {
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
            RegimentBlackboard.UpdateInformation();
            base.OnUpdate();
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
            /*
            if (order.StateOrdered == EStates.Fire)
            {
                ForceChangeState(order);
            }
*/
            RequestChangeState(order);
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
                    //Debug.Log("Added to Unit");
                    UnitsBehaviourTrees.Add(unitBt);
                    //unit.StateMachine.Initialize();
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
