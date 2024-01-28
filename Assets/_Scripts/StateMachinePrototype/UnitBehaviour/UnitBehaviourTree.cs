using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class UnitBehaviourTree : BehaviourTreeBase
    {
        public RegimentBehaviourTree RegimentBehaviourTree { get; private set; }
        public Unit UnitAttach { get; private set; }
        public EStates RegimentState => RegimentBehaviourTree.State;
        
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

        private void OnDestroy()
        {
            foreach (StateBase state in States.Values)
            {
                state.OnDestroy();
            }
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public override void RequestChangeState(Order order)
        {
            base.RequestChangeState(order);
        }

        private void Initializations()
        {
            UnitAttach = GetComponent<Unit>();
            RegimentBehaviourTree = UnitAttach.RegimentAttach.GetComponent<RegimentBehaviourTree>();
            InitializeStates();
        }
        
        private void InitializeStates()
        {
            States = new Dictionary<EStates, StateBase>()
            {
                {EStates.Idle, new Unit_IdleState(this)},
                {EStates.Move, new Unit_MoveState(this)},
                {EStates.Fire, new Unit_RangeAttackState(this)},
                //{EStates.Melee, new Unit_MeleeAttackState(this)},
            };
            State = EStates.Idle;
        }
    }
}
