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
            base.OnUpdate();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public void OnOrderReceived(Order order)
        {
            RequestChangeState(order);
        }
        
        public override void RequestChangeState(Order order)
        {
            EStates stateOrdered = order.StateOrdered;
            States[stateOrdered].OnSetup(order);
            Interruptions.Enqueue(stateOrdered); // Interruption a bouger dans la state machine?
            //Propagate Order to Units
            foreach (UnitBehaviourTree unitBehaviourTree in UnitsBehaviourTrees)
            {
                unitBehaviourTree.RequestChangeState(order);
            }
        }
        
        public void Initializations()
        {
            RegimentBlackboard = new Blackboard();
            RegimentAttach = GetComponent<Regiment>();
            InitializeStates();
            
            UnitsBehaviourTrees = new HashSet<UnitBehaviourTree>(RegimentAttach.Units.Count);
            DeadUnitsBehaviourTrees = new HashSet<UnitBehaviourTree>(RegimentAttach.Units.Count);
            foreach (Unit unit in RegimentAttach.Units)
            {
                UnitBehaviourTree unitBt = unit.AddUniqueComponent<UnitBehaviourTree>();
                UnitsBehaviourTrees.Add(unitBt);
                //unit.StateMachine.Initialize();
            }
        }
        
        private void InitializeStates()
        {
            
            States = new Dictionary<EStates, StateBase>()
            {
                {EStates.Idle, new Regiment_IdleState(this, RegimentBlackboard)},
                {EStates.Move, new Regiment_MoveState(this, RegimentBlackboard)},
                //{EStates.Fire, new FireRegimentState(this)},
                //{EStates.Melee, new ChaseRegimentState(this)},
            };
            
        }
    }
}
