using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class UnitBehaviourTree : BehaviourTreeBase
    {
        private RegimentBehaviourTree regimentBehaviourTree;
        public Unit UnitAttach { get; private set; }
        public EStates RegimentState => regimentBehaviourTree.State;
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
        private void Initializations()
        {
            UnitAttach = GetComponent<Unit>();
            regimentBehaviourTree = UnitAttach.RegimentAttach.GetComponent<RegimentBehaviourTree>();
            InitializeStates();
        }
        
        private void InitializeStates()
        {
            States = new Dictionary<EStates, StateBase>()
            {
                {EStates.Idle, new Unit_IdleState(this)},
                {EStates.Move, new Unit_MoveState(this)},
                {EStates.Fire, new Unit_RangeAttackState(this)},
                {EStates.Melee, new Unit_MeleeAttackState(this)},
            };
            State = EStates.Idle;
        }
    }
}
