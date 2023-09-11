using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWald.KzwMath;
using static Unity.Mathematics.math;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2;

namespace KaizerWald
{
    public class Regiment_RangeAttackState : RegimentStateBase
    {
        private Formation CurrentEnemyFormation => RegimentBlackboard.EnemyTarget.CurrentFormation;
        private int AttackRange => RegimentAttach.RegimentType.Range;
        
        public Regiment_RangeAttackState(RegimentBehaviourTree behaviourTree, Blackboard blackboard) : base(behaviourTree, blackboard, EStates.Fire)
        {
        }

        public override void OnSetup(Order order)
        {
            RangeAttackOrder rangeAttackOrder = (RangeAttackOrder)order;
            RegimentBlackboard.SetEnemyChase(rangeAttackOrder.TargetEnemyRegiment);
        }

        public override void OnEnter() { return; }

        public override void OnUpdate() { return; }

        public override void OnExit() { return; }

        public override EStates ShouldExit()
        {
            if (IdleExit()) return EStates.Idle;
            
            bool isEnemyInRange = StateExtension.CheckEnemiesAtRange(RegimentAttach, RegimentBlackboard.EnemyTarget, AttackRange);
            if (!isEnemyInRange) return ChaseExit() ? EStates.Move : EStates.Idle;
            
            return StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool IdleExit()
        {
            return !RegimentBlackboard.HasTarget;
        }
        
        private bool ChaseExit()
        {
            bool isChasing = RegimentBlackboard.IsChasing;
            if (isChasing) RegimentBlackboard.SetChaseDestination();
            return isChasing;
        }

        private bool HasEnemyFormationChange()
        {
            bool3 hasEnemyFormationChange = new bool3
            (
                RegimentBlackboard.CacheEnemyFormation.NumUnitsAlive != CurrentEnemyFormation.NumUnitsAlive,
                RegimentBlackboard.CacheEnemyFormation.WidthDepth != CurrentEnemyFormation.WidthDepth
            );
            return any(hasEnemyFormationChange);
        }
    }
}
