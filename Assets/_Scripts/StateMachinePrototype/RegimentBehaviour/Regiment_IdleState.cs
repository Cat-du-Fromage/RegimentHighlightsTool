using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWald.KzwMath;
using static Unity.Mathematics.math;

using static KaizerWald.StateExtension;
using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2;

namespace KaizerWald
{
    public sealed class Regiment_IdleState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool AutoFire { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private int AttackRange => RegimentAttach.RegimentType.Range;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setters ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void AutoFireOn() => AutoFire = true;
        public void AutoFireOff() => AutoFire = false;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment_IdleState(RegimentBehaviourTree behaviourTree, Blackboard blackBoard) : base(behaviourTree, blackBoard, EStates.Idle)
        {
        }

        //Maybe "Stop" button like in Total war?
        public override void OnSetup(Order order) { return; }

        public override void OnEnter() { return; }

        public override void OnUpdate() { return; }

        public override void OnExit() { return; }

        public override EStates ShouldExit()
        {
            if(FireExit()) return EStates.Fire;
            
            if (MoveExit()) return EStates.Move;
            
            return StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private bool FireExit()
        {
            bool enemyInRange = CheckEnemiesAtRange(RegimentAttach, AttackRange, out int targetId);
            if (enemyInRange) RegimentBlackboard.SetEnemyTarget(targetId);
            return enemyInRange;
        }
        
        private bool MoveExit()
        {
            bool isChasing = RegimentBlackboard.IsChasing;
            if (RegimentBlackboard.IsChasing) RegimentBlackboard.SetChaseDestination();
            return isChasing;
        }
    }
}
